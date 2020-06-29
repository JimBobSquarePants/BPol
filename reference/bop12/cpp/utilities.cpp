/***************************************************************************
 *   Developer: Francisco Martínez del Río (2012)                          *  
 *   fmartin@ujaen.es                                                      *
 *   Version: 1.0                                                          *
 *                                                                         *
 *   This is a public domain program                                       *
 ***************************************************************************/

#include <algorithm>
#include "utilities.h"

using namespace cbop;

static int findIntersection (double u0, double u1, double v0, double v1, double w[2])
{
	if ((u1 < v0) || (u0 > v1))
		return 0;
	if (u1 > v0) {
		if (u0 < v1) {
			w[0] = (u0 < v0) ? v0 : u0;
			w[1] = (u1 > v1) ? v1 : u1;
			return 2;
		} else {
			// u0 == v1
			w[0] = u0;
			return 1;
		}
	} else {
		// u1 == v0
		w[0] = u1;
		return 1;
	}
}

int cbop::findIntersection (const Segment_2& seg0, const Segment_2& seg1, Point_2& pi0, Point_2& pi1)
{
	Point_2 p0 = seg0.source ();
	Point_2 d0 (seg0.target ().x () - p0.x (), seg0.target ().y () - p0.y ());
	Point_2 p1 = seg1.source ();
	Point_2 d1 (seg1.target ().x () - p1.x (), seg1.target ().y () - p1.y ());
	double sqrEpsilon = 0.0000001; // it was 0.001 before
	Point_2 E (p1.x () - p0.x (), p1.y () - p0.y ());
	double kross = d0.x () * d1.y () - d0.y () * d1.x ();
	double sqrKross = kross * kross;
	double sqrLen0 = d0.x () * d0.x () + d0.y () * d0.y ();
	double sqrLen1 = d1.x () * d1.x () + d1.y () * d1.y ();

	if (sqrKross > sqrEpsilon * sqrLen0 * sqrLen1) {
		// lines of the segments are not parallel
		double s = (E.x () * d1.y () - E.y () * d1.x ()) / kross;
		if ((s < 0) || (s > 1)) {
			return 0;
		}
		double t = (E.x () * d0.y () - E.y () * d0.x ()) / kross;
		if ((t < 0) || (t > 1)) {
			return 0;
		}
		// intersection of lines is a point an each segment
		pi0 = Point_2 (p0.x () + s * d0.x (), p0.y () + s * d0.y ());
		if (pi0.dist (seg0.source ()) < 0.00000001) pi0 = seg0.source ();
		if (pi0.dist (seg0.target ()) < 0.00000001) pi0 = seg0.target ();
		if (pi0.dist (seg1.source ()) < 0.00000001) pi0 = seg1.source ();
		if (pi0.dist (seg1.target ()) < 0.00000001) pi0 = seg1.target ();
		return 1;
	}

	// lines of the segments are parallel
	double sqrLenE = E.x () * E.x () + E.y () * E.y ();
	kross = E.x () * d0.y () - E.y () * d0.x ();
	sqrKross = kross * kross;
	if (sqrKross > sqrEpsilon * sqrLen0 * sqrLenE) {
		// lines of the segment are different
		return 0;
	}

	// Lines of the segments are the same. Need to test for overlap of segments.
	double s0 = (d0.x () * E.x () + d0.y () * E.y ()) / sqrLen0;  // so = Dot (D0, E) * sqrLen0
	double s1 = s0 + (d0.x () * d1.x () + d0.y () * d1.y ()) / sqrLen0;  // s1 = s0 + Dot (D0, D1) * sqrLen0
	double smin = std::min (s0, s1);
	double smax = std::max (s0, s1);
	double w[2];
	int imax = ::findIntersection (0.0, 1.0, smin, smax, w);

	if (imax > 0) {
		pi0 = Point_2 (p0.x () + w[0] * d0.x (), p0.y () + w[0] * d0.y ());
		if (pi0.dist (seg0.source ()) < 0.00000001) pi0 = seg0.source ();
		if (pi0.dist (seg0.target ()) < 0.00000001) pi0 = seg0.target ();
		if (pi0.dist (seg1.source ()) < 0.00000001) pi0 = seg1.source ();
		if (pi0.dist (seg1.target ()) < 0.00000001) pi0 = seg1.target ();
		if (imax > 1)
			pi1 = Point_2 (p0.x () + w[1] * d0.x (), p0.y () + w[1] * d0.y ());
	}
	return imax;
}
