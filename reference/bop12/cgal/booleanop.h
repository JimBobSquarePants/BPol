#ifndef BOOLEANOP_H
#define BOOLEANOP_H

#include <vector>
#include <list>
#include <string>
#include <set>
#include <queue>
#include <functional>
#include <iostream>
#ifdef __STEPBYSTEP
	#include <QThread>
	#include <QSemaphore>
#endif
#include "polygon.h"

namespace bop {

enum BooleanOpType { INTERSECTION, UNION, DIFFERENCE, XOR };
enum EdgeType { NORMAL, NON_CONTRIBUTING, SAME_TRANSITION, DIFFERENT_TRANSITION };
enum PolygonType { SUBJECT, CLIPPING };

struct SweepEvent; // forward declaration
struct SegmentComp : public std::binary_function<SweepEvent*, SweepEvent*, bool> { // for sorting edges in the sweep line (sl)
	bool operator() (SweepEvent* le1, SweepEvent* le2);
};

struct SweepEvent {
	SweepEvent () {}
	SweepEvent (bool b, const Point_2& p, SweepEvent* other, PolygonType pt, EdgeType et = NORMAL);
	bool left;              // is point the left endpoint of the edge (point, otherEvent->point)?
	Point_2 point;          // point associated with the event
	Line_2 line;            // oriented line associated to the edge (point, otherEvent->point)
	SweepEvent* otherEvent; // event associated to the other endpoint of the edge
	PolygonType pol;        // Polygon to which the associated segment belongs to
	EdgeType type;
	//The following fields are only used in "left" events
	/**  Does segment (point, otherEvent->p) represent an inside-outside transition in the polygon for a vertical ray from (p.x, -infinite)? */
	bool inOut;
	bool otherInOut; // inOut transition for the segment from the other polygon preceding this segment in sl
	std::set<SweepEvent*, SegmentComp>::iterator posSL; // Position of the event (line segment) in sl
	SweepEvent* prevInResult; // previous segment in sl belonging to the result of the boolean operation
	bool inResult;
	// The following fields are used in the second stage of the algorithm (connectEdges function)
	unsigned int pos;
	bool resultInOut;
	unsigned int contourId;
	// member functions
	/** Is the line segment (point, otherEvent->point) below point p */
	bool below (const Point_2& p) const { return line.has_on_positive_side (p); }
	/** Is the line segment (point, otherEvent->point) above point p */
	bool above (const Point_2& p) const { return line.has_on_negative_side (p); }
	/** Is the line segment (point, otherEvent->point) a vertical line segment */
	bool vertical () const { return point.x () == otherEvent->point.x (); }
	/** Return the line segment associated to the SweepEvent */
	Segment_2 segment () const { return Segment_2 (point, otherEvent->point); }
	std::string toString () const;
};

struct SweepEventComp : public std::binary_function<SweepEvent, SweepEvent, bool> { // for sorting sweep events
	bool operator() (const SweepEvent* e1, const SweepEvent* e2);
};

class BooleanOpImp
#ifdef __STEPBYSTEP
 : public QThread
#endif
{
public:
	BooleanOpImp (const Polygon& subj, const Polygon& clip, Polygon& result, BooleanOpType op
#ifdef __STEPBYSTEP
,QSemaphore* ds = 0, QSemaphore* sd = 0, bool trace = false
#endif
);
	void run ();

#ifdef __STEPBYSTEP
	typedef std::set<SweepEvent*, SegmentComp>::const_iterator const_sl_iterator;
	typedef std::deque<SweepEvent*>::const_iterator const_sortedEvents_iterator;
	typedef std::vector<SweepEvent*>::const_iterator const_out_iterator;
	const_sl_iterator beginSL () const { return sl.begin (); }
	const_sl_iterator endSL () const { return sl.end (); }
	const_sortedEvents_iterator beginSortedEvents () const { return sortedEvents.begin (); }
	const_sortedEvents_iterator endSortedEvents () const { return sortedEvents.end (); }
	SweepEvent* currentEvent () const { return _currentEvent; }
	SweepEvent* previousEvent () const { return _previousEvent; }
	SweepEvent* nextEvent () const { return _nextEvent; }
	Point_2 currentPoint () const { return _currentPoint; }
	const_out_iterator beginOut () const { return out.begin (); }
	const_out_iterator endOut () const { return out.end (); }
#endif
private:
	const Polygon& subject;
	const Polygon& clipping;
	Polygon& result;
	BooleanOpType operation;
	std::priority_queue<SweepEvent*, std::vector<SweepEvent*>, SweepEventComp> eq; // event queue (sorted events to be processed)
	std::set<SweepEvent*, SegmentComp> sl; // segments intersecting the sweep line
	std::deque<SweepEvent> eventHolder;    // It holds the events generated during the computation of the boolean operation
	SweepEventComp sec;                    // to compare events
	std::deque<SweepEvent*> sortedEvents;
	bool trivialOperation (const CGAL::Bbox_2& subjectBB, const CGAL::Bbox_2& clippingBB);
	/** @brief Compute the events associated to segment s, and insert them into pq and eq */
	void processSegment (const Segment_2& s, PolygonType pt);
	/** @brief Store the SweepEvent e into the event holder, returning the address of e */
	SweepEvent *storeSweepEvent (const SweepEvent& e) { eventHolder.push_back (e); return &eventHolder.back (); }
	/** @brief Process a posible intersection between the edges associated to the left events le1 and le2 */
	int possibleIntersection (SweepEvent* le1, SweepEvent* le2);
	/** @brief Divide the segment associated to left event le, updating pq and (implicitly) the status line */
	void divideSegment (SweepEvent* le, const Point_2& p);
	/** @brief return if the left event le belongs to the result of the Boolean operation */
	bool inResult (SweepEvent* le);
	/** @brief compute several fields of left event le */
	void computeFields (SweepEvent* le, const std::set<SweepEvent*, SegmentComp>::iterator& prev);
	// connect the solution edges to form the result polygon
	void connectEdges ();
	int nextPos (int pos, const std::vector<SweepEvent*>& resultEvents, const std::vector<bool>& processed);

#ifdef __STEPBYSTEP
	bool trace;
	SweepEvent* _currentEvent;
	SweepEvent* _previousEvent;
	SweepEvent* _nextEvent;
	Point_2 _currentPoint;
	QSemaphore* doSomething;
	QSemaphore* somethingDone;
	std::vector<SweepEvent*> out;
#endif
};

inline void compute (const Polygon& subj, const Polygon& clip, Polygon& result, BooleanOpType op)
{
	BooleanOpImp boi (subj, clip, result, op);
	boi.run ();
}

} // end of namespace bop
#endif
