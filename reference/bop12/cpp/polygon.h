/***************************************************************************
 *   Developer: Francisco Martínez del Río (2012)                          *  
 *   fmartin@ujaen.es                                                      *
 *   Version: 1.0                                                          *
 *                                                                         *
 *   This is a public domain program                                       *
 ***************************************************************************/

#ifndef POLYGON_H
#define POLYGON_H

#include <vector>
#include <algorithm>
#include "utilities.h"
#include "bbox_2.h"

namespace cbop {

class Contour {
public:
	typedef std::vector<Point_2>::iterator iterator;
	typedef std::vector<Point_2>::const_iterator const_iterator;
	
	Contour () : points (), holes (), _external (true), _precomputedCC (false) {}

	/** Get the p-th vertex of the external contour */
	Point_2& vertex (unsigned int p) { return points[p]; }
	Point_2 vertex (unsigned int p) const { return points[p]; }
	Segment_2 segment (unsigned p) const { return (p == nvertices () - 1) 
		? Segment_2 (points.back (), points.front ()) 
		: Segment_2 (points[p], points[p+1]); }
	/** Number of vertices and edges */
	unsigned nvertices () const { return points.size (); }
	unsigned nedges () const { return points.size (); }
	/** Get the bounding box */
	Bbox_2 bbox () const;
	/** Return if the contour is counterclockwise oriented */
	bool counterclockwise ();
	/** Return if the contour is clockwise oriented */
	bool clockwise () { return !counterclockwise (); }
	void changeOrientation () { std::reverse (points.begin (), points.end ()); _CC = !_CC; }
	void setClockwise () { if (counterclockwise ()) changeOrientation (); }
	void setCounterClockwise () { if (clockwise ()) changeOrientation (); }

	void move (double x, double y);
	void add (const Point_2& s) { points.push_back (s); }
	void erase (iterator i) { points.erase (i); }
	void clear () { points.clear (); holes.clear (); }
	void clearHoles () { holes.clear (); }
	iterator begin () { return points.begin (); }
	iterator end () { return points.end (); }
	const_iterator begin () const { return points.begin (); }
	const_iterator end () const { return points.end (); }
	Point_2& back () { return points.back (); }
	const Point_2& back () const { return points.back (); }
	void addHole (unsigned int ind) { holes.push_back (ind); }
	unsigned int nholes () const { return holes.size (); }
	unsigned int hole (unsigned int p) const { return holes[p]; }
	bool external () const { return _external; }
	void setExternal (bool e) { _external = e; }

	private:
	/** Set of points conforming the external contour */
	std::vector<Point_2> points;
	/** Holes of the contour. They are stored as the indexes of the holes in a polygon class */
	std::vector<unsigned int> holes;
	bool _external; // is the contour an external contour? (i.e., is it not a hole?)
	bool _precomputedCC;
	bool _CC;
};

std::ostream& operator<< (std::ostream& o, Contour& c);

class Polygon {
public:
	typedef std::vector<Contour>::iterator iterator;
	typedef std::vector<Contour>::const_iterator const_iterator;
	
	Polygon () : contours () {}

	// Get the polygon from a text file */
	bool open (const std::string& filename);
	void join (const Polygon& pol);
	/** Get the p-th contour */
	Contour& contour (unsigned int p) { return contours[p]; }
	const Contour& contour (unsigned int p) const { return contours[p]; }
	Contour& operator[] (unsigned int p) { return contours[p]; }
	const Contour& operator[] (unsigned int p) const { return contours[p]; }
	/** Number of contours */
	unsigned int ncontours () const { return contours.size (); }
	/** Number of vertices */
	unsigned int nvertices () const;
	/** Get the bounding box */
	Bbox_2 bbox () const;

	void move (double x, double y);

	void push_back (const Contour& c) { contours.push_back (c); } 
	Contour& back () { return contours.back (); }
	const Contour& back () const { return contours.back (); }
	void pop_back () { contours.pop_back (); }
	void erase (iterator i) { contours.erase (i); }
	void clear () { contours.clear (); }

	iterator begin () { return contours.begin (); }
	iterator end () { return contours.end (); }
	const_iterator begin () const { return contours.begin (); }
	const_iterator end () const { return contours.end (); }
	void computeHoles ();
private:
	/** Set of contours conforming the polygon */
	std::vector<Contour> contours;
};

std::ostream& operator<< (std::ostream& o, Polygon& p);
std::istream& operator>> (std::istream& i, Polygon& p);

} // end of namespace cbop
#endif
