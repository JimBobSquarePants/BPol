/***************************************************************************
 *   Developer: Francisco Martínez del Río (2012)                          *  
 *   fmartin@ujaen.es                                                      *
 *   Version: 1.0                                                          *
 *                                                                         *
 *   This is a public domain program                                       *
 ***************************************************************************/

// ------------------------------------------------------------------
// Segment_2 Class - A line segment in the plane
// ------------------------------------------------------------------

#ifndef SEGMENT_2_H
#define SEGMENT_2_H

#include <algorithm>
#include "point_2.h"

namespace cbop {

class Segment_2 {
public:
	/** Default constructor */
	Segment_2 () {}

	/** Constructor from two points **/
	Segment_2 (const Point_2& source, const Point_2& target) : s (source), t (target) {}

	/** Set the source point */
	void setSource (const Point_2& source) { s = source; }
	/** Set the target point */
	void setTarget (const Point_2& target) { t = target; }

	/** Get the source point */
	const Point_2& source () const { return s; }
	/** Get the target point */
	const Point_2& target () const { return t; }

	/** Return the point of the segment with lexicographically smallest coordinate */
	const Point_2& min () const { return (s.x () < t.x ()) || (s.x () == t.x () && s.y () < t.y ()) ? s : t; }
	/** Return the point of the segment with lexicographically largest coordinate */
	const Point_2& max () const { return (s.x () > t.x ()) || (s.x () == t.x () && s.y () > t.y ()) ? s : t; }
	bool degenerate () const { return s == t; }
	bool is_vertical () const { return s.x () == t.x (); }
	/** Change the segment orientation */
	Segment_2& changeOrientation () { std::swap (s, t); return *this; }
private:
	/** Segment endpoints */
	Point_2 s, t;
};

inline std::ostream& operator<< (std::ostream& o, const Segment_2& p) {
	return o << p.source () << "-" << p.target ();
}

} // end of namespace cbop
#endif
