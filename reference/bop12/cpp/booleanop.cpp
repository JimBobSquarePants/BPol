/***************************************************************************
 *   Developer: Francisco Martínez del Río (2012)                          *  
 *   fmartin@ujaen.es                                                      *
 *   Version: 1.0                                                          *
 *                                                                         *
 *   This is a public domain program                                       *
 ***************************************************************************/

#include <cstdlib>
#include <fstream>
#include <sstream>
#include <algorithm>
#include "booleanop.h"

using namespace cbop;

SweepEvent::SweepEvent (bool b, const Point_2& p, SweepEvent* other, PolygonType pt, EdgeType et) : 
  left (b), point (p), otherEvent (other), pol (pt), type (et), prevInResult (0), inResult (false)
{
}

std::string SweepEvent::toString () const
{
	std::ostringstream oss;
	oss << '(' << point.x () << ',' << point.y () << ')';
	oss << " (" << (left ? "left" : "right") << ')';
	Segment_2 s (point, otherEvent->point);
	oss << " S:[(" << s.min ().x () << ',' << s.min ().y () << ") - (" << s.max ().x () << ',' << s.max ().y () << ")]";
	oss << " (" << (pol == SUBJECT ? "SUBJECT" : "CLIPPING") << ')';
	std::string et[4] =  { "NORMAL", "NON_CONTRIBUTING", "SAME_TRANSITION", "DIFFERENT_TRANSITION" };
	oss << " (" << et[type] << ')';
	oss << " (" << (inOut ? "inOut" : "outIn") << ')';
	oss << " otherInOut: (" << (otherInOut ? "inOut" : "outIn") << ')';
	return oss.str ();
}

// le1 and le2 are the left events of line segments (le1->point, le1->otherEvent->point) and (le2->point, le2->otherEvent->point)
bool SegmentComp::operator() (SweepEvent* le1, SweepEvent* le2)
{
	if (le1 == le2)
		return false;
	if (signedArea (le1->point, le1->otherEvent->point, le2->point) != 0 || 
		signedArea (le1->point, le1->otherEvent->point, le2->otherEvent->point) != 0) {
		// Segments are not collinear
		// If they share their left endpoint use the right endpoint to sort
		if (le1->point == le2->point)
			return le1->below (le2->otherEvent->point);
		// Different left endpoint: use the left endpoint to sort
		if (le1->point.x () == le2->point.x ())
			return le1->point.y () < le2->point.y ();
		SweepEventComp comp;
		if (comp (le1, le2))  // has the line segment associated to e1 been inserted into S after the line segment associated to e2 ?
			return le2->above (le1->point);
		// The line segment associated to e2 has been inserted into S after the line segment associated to e1
		return le1->below (le2->point);
	}
	// Segments are collinear
	if (le1->pol != le2->pol)
		return le1->pol < le2->pol;
	// Just a consistent criterion is used
	if (le1->point == le2->point)
		return le1 < le2;
	SweepEventComp comp;
	return comp (le1, le2);
}

BooleanOpImp::BooleanOpImp (const Polygon& subj, const Polygon& clip, Polygon& res, BooleanOpType op 
#ifdef __STEPBYSTEP
, QSemaphore* ds, QSemaphore* sd, bool t
#endif
) : subject (subj), clipping (clip), result (res), operation (op), eq (), sl (), eventHolder ()
#ifdef __STEPBYSTEP
, trace (t), _currentEvent (0), _previousEvent (0), _nextEvent (0), doSomething (ds), somethingDone (sd), out ()
#endif
{
}

void BooleanOpImp::run ()
{
	Bbox_2 subjectBB = subject.bbox ();     // for optimizations 1 and 2
	Bbox_2 clippingBB = clipping.bbox ();   // for optimizations 1 and 2
	const double MINMAXX = std::min (subjectBB.xmax (), clippingBB.xmax ()); // for optimization 2
	if (trivialOperation (subjectBB, clippingBB)) // trivial cases can be quickly resolved without sweeping the plane
		return;
	for (unsigned int i = 0; i < subject.ncontours (); i++)
		for (unsigned int j = 0; j < subject.contour (i).nvertices (); j++)
			processSegment (subject.contour (i).segment (j), SUBJECT);
	for (unsigned int i = 0; i < clipping.ncontours (); i++)
		for (unsigned int j = 0; j < clipping.contour (i).nvertices (); j++)
			processSegment (clipping.contour (i).segment (j), CLIPPING);

	std::set<SweepEvent*, SegmentComp>::iterator it, prev, next;
	
	while (! eq.empty ()) {
		SweepEvent* se = eq.top ();
		// optimization 2
		if ((operation == INTERSECTION && se->point.x () > MINMAXX) ||
			(operation == DIFFERENCE && se->point.x () > subjectBB.xmax ())) {
			connectEdges ();
			return;
		}
		sortedEvents.push_back (se);
#ifdef __STEPBYSTEP
		if (trace) {
			doSomething->acquire ();
			_currentPoint = se->point;
		}
#endif
		eq.pop ();
		if (se->left) { // the line segment must be inserted into sl
			next = prev = se->posSL = it = sl.insert(se).first;
			(prev != sl.begin()) ? --prev : prev = sl.end();
			++next;
#ifdef __STEPBYSTEP
			if (trace) {
				_currentEvent = *it;
				_previousEvent = prev != sl.end () ? *prev : 0;
				_nextEvent = next != sl.end () ? *next : 0;
			}
#endif
			computeFields (se, prev);
			// Process a possible intersection between "se" and its next neighbor in sl
			if (next != sl.end()) {
				if (possibleIntersection(se, *next) == 2) {
					computeFields (se, prev);
					computeFields (*next, it);
				}
			}
			// Process a possible intersection between "se" and its previous neighbor in sl
			if (prev != sl.end ()) {
				if (possibleIntersection(*prev, se) == 2) {
					std::set<SweepEvent*, SegmentComp>::iterator prevprev = prev;
					(prevprev != sl.begin()) ? --prevprev : prevprev = sl.end();
					computeFields (*prev, prevprev);
					computeFields (se, prev);
				}
			}
		} else { // the line segment must be removed from sl
			se = se->otherEvent; // we work with the left event
			next = prev = it = se->posSL; // se->posSL; is equal than sl.find (se); but faster
			(prev != sl.begin()) ? --prev : prev = sl.end();
			++next;
#ifdef __STEPBYSTEP
			if (trace) {
				_currentEvent = *it;
				_previousEvent = prev != sl.end () ? *prev : 0;
				_nextEvent = next != sl.end () ? *next : 0;
			}
#endif
			// delete line segment associated to "se" from sl and check for intersection between the neighbors of "se" in sl
			sl.erase (it);
			if (next != sl.end() && prev != sl.end())
				possibleIntersection (*prev, *next);
		}
#ifdef __STEPBYSTEP
		if (trace)
			somethingDone->release ();
#endif
	}
	connectEdges ();
}

bool BooleanOpImp::trivialOperation (const Bbox_2& subjectBB, const Bbox_2& clippingBB)
{
	// Test 1 for trivial result case
	if (subject.ncontours () * clipping.ncontours () == 0) { // At least one of the polygons is empty
		if (operation == DIFFERENCE)
			result = subject;
		if (operation == UNION || operation == XOR)
			result = (subject.ncontours () == 0) ? clipping : subject;
		return true;
	}
	// Test 2 for trivial result case
	if (subjectBB.xmin () > clippingBB.xmax () || clippingBB.xmin () > subjectBB.xmax () ||
		subjectBB.ymin () > clippingBB.ymax () || clippingBB.ymin () > subjectBB.ymax ()) {
		// the bounding boxes do not overlap
		if (operation == DIFFERENCE)
			result = subject;
		if (operation == UNION || operation == XOR) {
			result = subject;
			result.join (clipping);
		}
		return true;
	}
	return false;
}

void BooleanOpImp::processSegment (const Segment_2& s, PolygonType pt)
{
/*	if (s.degenerate ()) // if the two edge endpoints are equal the segment is dicarded
		return;          // This can be done as preprocessing to avoid "polygons" with less than 3 edges */
	SweepEvent* e1 = storeSweepEvent (SweepEvent(true, s.source (), 0, pt));
	SweepEvent* e2 = storeSweepEvent (SweepEvent(true, s.target (), e1, pt));
	e1->otherEvent = e2;

	if (s.min () == s.source ()) {
		e2->left = false;
	} else {
		e1->left = false;
	}
	eq.push (e1);
	eq.push (e2);
}

void BooleanOpImp::computeFields (SweepEvent* le, const std::set<SweepEvent*, SegmentComp>::iterator& prev)
{
	// compute inOut and otherInOut fields
	if (prev == sl.end ()) {
		le->inOut = false;
		le->otherInOut = true;
	} else if (le->pol == (*prev)->pol) { // previous line segment in sl belongs to the same polygon that "se" belongs to
		le->inOut = ! (*prev)->inOut;
		le->otherInOut = (*prev)->otherInOut;
	} else {                          // previous line segment in sl belongs to a different polygon that "se" belongs to
		le->inOut = ! (*prev)->otherInOut;
		le->otherInOut = (*prev)->vertical () ? ! (*prev)->inOut : (*prev)->inOut;
	}
	// compute prevInResult field
	if (prev != sl.end ())
		le->prevInResult = (!inResult (*prev) || (*prev)->vertical ()) ? (*prev)->prevInResult : *prev;
	// check if the line segment belongs to the Boolean operation
	le->inResult = inResult (le);
}

bool BooleanOpImp::inResult (SweepEvent* le)
{
	switch (le->type) {
		case NORMAL:
			switch (operation) {
				case (INTERSECTION):
					return ! le->otherInOut;
				case (UNION):
					return le->otherInOut;
				case (DIFFERENCE):
					return (le->pol == SUBJECT && le->otherInOut) || (le->pol == CLIPPING && !le->otherInOut);
				case (XOR):
					return true;
			}
		case SAME_TRANSITION:
			return operation == INTERSECTION || operation == UNION;
		case DIFFERENT_TRANSITION:
			return operation == DIFFERENCE;
		case NON_CONTRIBUTING:
			return false;
	}
	return false; // just to avoid the compiler warning
}

int BooleanOpImp::possibleIntersection (SweepEvent* le1, SweepEvent* le2)
{
//	if (e1->pol == e2->pol) // you can uncomment these two lines if self-intersecting polygons are not allowed
//		return 0;

	Point_2 ip1, ip2;  // intersection points
	int nintersections;

	if (!(nintersections = findIntersection(le1->segment (), le2->segment (), ip1, ip2)))
		return 0;  // no intersection

	if ((nintersections == 1) && ((le1->point == le2->point) || (le1->otherEvent->point == le2->otherEvent->point)))
		return 0; // the line segments intersect at an endpoint of both line segments

	if (nintersections == 2 && le1->pol == le2->pol) {
		std::cerr << "Sorry, edges of the same polygon overlap\n";
		exit (1); // the line segments overlap, but they belong to the same polygon
	}

	// The line segments associated to le1 and le2 intersect
	if (nintersections == 1) {
		if (le1->point != ip1 && le1->otherEvent->point != ip1)  // if the intersection point is not an endpoint of le1->segment ()
			divideSegment (le1, ip1);
		if (le2->point != ip1 && le2->otherEvent->point != ip1)  // if the intersection point is not an endpoint of le2->segment ()
			divideSegment (le2, ip1);
		return 1;
	}
	// The line segments associated to le1 and le2 overlap
	std::vector<SweepEvent*> sortedEvents;
	if (le1->point == le2->point) {
		sortedEvents.push_back (0);
	} else if (sec (le1, le2)) {
		sortedEvents.push_back (le2);
		sortedEvents.push_back (le1);
	} else {
		sortedEvents.push_back (le1);
		sortedEvents.push_back (le2);
	}
	if (le1->otherEvent->point == le2->otherEvent->point) {
		sortedEvents.push_back (0);
	} else if (sec (le1->otherEvent, le2->otherEvent)) {
		sortedEvents.push_back (le2->otherEvent);
		sortedEvents.push_back (le1->otherEvent);
	} else {
		sortedEvents.push_back (le1->otherEvent);
		sortedEvents.push_back (le2->otherEvent);
	}

	if ((sortedEvents.size () == 2) || (sortedEvents.size () == 3 && sortedEvents[2])) { 
		// both line segments are equal or share the left endpoint
		le1->type = NON_CONTRIBUTING;
		le2->type = (le1->inOut == le2->inOut) ? SAME_TRANSITION : DIFFERENT_TRANSITION;
		if (sortedEvents.size () == 3)
			divideSegment (sortedEvents[2]->otherEvent, sortedEvents[1]->point);
		return 2;
	}
	if (sortedEvents.size () == 3) { // the line segments share the right endpoint
		divideSegment (sortedEvents[0], sortedEvents[1]->point);
		return 3;
	}
	if (sortedEvents[0] != sortedEvents[3]->otherEvent) { // no line segment includes totally the other one
		divideSegment (sortedEvents[0], sortedEvents[1]->point);
		divideSegment (sortedEvents[1], sortedEvents[2]->point);
		return 3;
	}
	 // one line segment includes the other one
	divideSegment (sortedEvents[0], sortedEvents[1]->point);
	divideSegment (sortedEvents[3]->otherEvent, sortedEvents[2]->point);
	return 3;
}

void BooleanOpImp::divideSegment (SweepEvent* le, const Point_2& p)
{
//	std::cout << "YES. INTERSECTION" << std::endl;
	// "Right event" of the "left line segment" resulting from dividing le->segment ()
	SweepEvent* r = storeSweepEvent (SweepEvent (false, p, le, le->pol/*, le->type*/));
	// "Left event" of the "right line segment" resulting from dividing le->segment ()
	SweepEvent* l = storeSweepEvent (SweepEvent (true, p, le->otherEvent, le->pol/*, le->other->type*/));
	if (sec (l, le->otherEvent)) { // avoid a rounding error. The left event would be processed after the right event
		std::cout << "Oops" << std::endl;
		le->otherEvent->left = true;
		l->left = false;
	}
	if (sec (le, r)) { // avoid a rounding error. The left event would be processed after the right event
		std::cout << "Oops2" << std::endl;
	}
	le->otherEvent->otherEvent = l;
	le->otherEvent = r;
	eq.push (l);
	eq.push (r);
}

void BooleanOpImp::connectEdges ()
{
	// copy the events in the result polygon to resultEvents array
	std::vector<SweepEvent*> resultEvents;
	resultEvents.reserve (sortedEvents.size ());
	for (std::deque<SweepEvent*>::const_iterator it = sortedEvents.begin (); it != sortedEvents.end (); it++)
		if (((*it)->left && (*it)->inResult) || (!(*it)->left && (*it)->otherEvent->inResult))
			resultEvents.push_back (*it);

	// Due to overlapping edges the resultEvents array can be not wholly sorted
	bool sorted = false;
	while (!sorted) {
		sorted = true;
		for (unsigned int i = 0; i < resultEvents.size (); ++i) {
			if (i + 1 < resultEvents.size () && sec (resultEvents[i], resultEvents[i+1])) {
				std::swap (resultEvents[i], resultEvents[i+1]);
				sorted = false;
			}
		}
	}

	for (unsigned int i = 0; i < resultEvents.size (); ++i) {
		resultEvents[i]->pos = i;
		if (!resultEvents[i]->left)
			std::swap (resultEvents[i]->pos, resultEvents[i]->otherEvent->pos);
	}

	std::vector<bool> processed (resultEvents.size (), false);
	std::vector<int> depth;
	std::vector<int> holeOf;
	for (unsigned int i = 0; i < resultEvents.size (); i++) {
		if (processed[i])
			continue;
		result.push_back (Contour ());
		Contour& contour = result.back ();
		unsigned int contourId = result.ncontours () - 1;
		depth.push_back (0);
		holeOf.push_back (-1);
		if (resultEvents[i]->prevInResult) {
			unsigned int lowerContourId = resultEvents[i]->prevInResult->contourId;
			if (!resultEvents[i]->prevInResult->resultInOut) {
				result[lowerContourId].addHole (contourId);
				holeOf[contourId] = lowerContourId;
				depth[contourId] = depth[lowerContourId] + 1;
				contour.setExternal (false);
			} else if (!result[lowerContourId].external ()) {
				result[holeOf[lowerContourId]].addHole (contourId);
				holeOf[contourId] = holeOf[lowerContourId];
				depth[contourId] = depth[lowerContourId];
				contour.setExternal (false);
			}
		}
		int pos = i;
		Point_2 initial = resultEvents[i]->point;
		contour.add (initial);
		while (resultEvents[pos]->otherEvent->point != initial) {
#ifdef __STEPBYSTEP
			if (trace) {
				doSomething->acquire ();
				out.push_back (resultEvents[pos]->left ? resultEvents[pos] : resultEvents[pos]->otherEvent);
			}
#endif
			processed[pos] = true; 
			if (resultEvents[pos]->left) {
				resultEvents[pos]->resultInOut = false;
				resultEvents[pos]->contourId = contourId;
			} else {
				resultEvents[pos]->otherEvent->resultInOut = true; 
				resultEvents[pos]->otherEvent->contourId = contourId;
			}
			processed[pos = resultEvents[pos]->pos] = true; 
			contour.add (resultEvents[pos]->point);
			pos = nextPos (pos, resultEvents, processed);
#ifdef __STEPBYSTEP
			if (trace)
				somethingDone->release ();
#endif
		}
#ifdef __STEPBYSTEP
		if (trace)
			out.push_back (resultEvents[pos]->left ? resultEvents[pos] : resultEvents[pos]->otherEvent);
#endif
		processed[pos] = processed[resultEvents[pos]->pos] = true;
		resultEvents[pos]->otherEvent->resultInOut = true; 
		resultEvents[pos]->otherEvent->contourId = contourId;
		if (depth[contourId] & 1)
			contour.changeOrientation ();
	}
}

int BooleanOpImp::nextPos (int pos, const std::vector<SweepEvent*>& resultEvents, const std::vector<bool>& processed)
{
	unsigned int newPos = pos + 1;
	while (newPos < resultEvents.size () && resultEvents[newPos]->point == resultEvents[pos]->point) {
		if (!processed[newPos])
			return newPos;
		else
			++newPos;
	}
	newPos = pos - 1;
	while (processed[newPos])
		--newPos;
	return newPos;
}
