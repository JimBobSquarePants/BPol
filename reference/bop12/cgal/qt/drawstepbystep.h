#ifndef DRAWSTEPBYSTEP_H
#define DRAWSTEPBYSTEP_H

#include <QGLWidget>
#include <GL/gl.h>
#include <vector>
#include <QSemaphore>
#include "../booleanop.h"
#include <CGAL/Simple_cartesian.h>
#include <CGAL/Cartesian_converter.h>

typedef CGAL::Exact_predicates_exact_constructions_kernel EK;
typedef CGAL::Simple_cartesian<double>                    IK;
typedef CGAL::Cartesian_converter<EK,IK>                  EK_to_IK;

using namespace bop;

class QLineEdit;

class DrawStepByStep : public QGLWidget
{
	Q_OBJECT
public:
	DrawStepByStep (const Polygon& subj, const Polygon& clip, bop::BooleanOpImp* boi, QWidget* parent = 0);
protected:
	void initializeGL ();
	void resizeGL (int width, int height);
	void paintGL ();
	void keyPressEvent (QKeyEvent *e);

private:
	const Polygon& subject;
	const Polygon& clipping;
	bop::BooleanOpImp* boi;
	GLdouble zoom;
	GLdouble xoffset;
	GLdouble yoffset;
	GLdouble width;
	GLdouble height;
	CGAL::Bbox_2 bb;
	int w, h; // window size 
	EK_to_IK to_inexact; 
	void draw ();
	IK::Point_2 inexactPoint (const IK::Point_2& p) const { return p; }
	IK::Point_2 inexactPoint (const EK::Point_2& p) const { EK_to_IK toInexact; return toInexact (p); }
	void drawPoint (const IK::Point_2& p) const { glVertex2d (p.x (), p.y ()); }
	void drawSegment (const bop::SweepEvent* se) const;
};

#endif
