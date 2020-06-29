#ifndef DRAWSTEPBYSTEP_H
#define DRAWSTEPBYSTEP_H

#include <QGLWidget>
#include <GL/gl.h>
#include <vector>
#include <QSemaphore>
#include "../booleanop.h"

class QLineEdit;

class DrawStepByStep : public QGLWidget
{
	Q_OBJECT
public:
	DrawStepByStep (const cbop::Polygon& subj, const cbop::Polygon& clip, cbop::BooleanOpImp* boi, QWidget* parent = 0);
protected:
	void initializeGL ();
	void resizeGL (int width, int height);
	void paintGL ();
	void keyPressEvent (QKeyEvent *e);

private:
	const cbop::Polygon& subject;
	const cbop::Polygon& clipping;
	cbop::BooleanOpImp* boi;
	GLdouble zoom;
	GLdouble xoffset;
	GLdouble yoffset;
	GLdouble width;
	GLdouble height;
	cbop::Bbox_2 bb;
	int w, h; // window size 
	void draw ();
	void drawPoint (const cbop::Point_2& p) const { glVertex2d (p.x (), p.y ()); }
	void drawSegment (const cbop::SweepEvent* se) const;
};

#endif
