#include <GL/gl.h>
#include <QtOpenGL>
#include <iostream>
#include "drawstepbystep.h"

using namespace cbop;

DrawStepByStep::DrawStepByStep (const Polygon& subj, const Polygon& clip, cbop::BooleanOpImp* boip, QWidget* parent) : 
QGLWidget (parent), subject (subj), clipping (clip), boi (boip), zoom (-0.05), xoffset (0), yoffset (0)
{
	setFocusPolicy (Qt::ClickFocus);
}

void DrawStepByStep::initializeGL ()
{
	glClearColor (0, 0, 0, 0);
	glColor3f (1, 1, 1);
	glPointSize (5.0);
	glLineStipple (1, 0x0FFF);
}

void DrawStepByStep::resizeGL (int wi, int he)
{
	w = wi;
	h = he;
	bb = subject.bbox () + clipping.bbox ();
	width = bb.xmax () - bb.xmin ();
	height = bb.ymax () - bb.ymin ();
	glMatrixMode (GL_PROJECTION);
	glLoadIdentity ();
	gluOrtho2D (bb.xmin () + width*zoom, bb.xmax () - width*zoom, bb.ymin () + height*zoom, bb.ymax () - height*zoom);
	if (height*h == 0)
		return;
	if (width/height > (float) w / h)
		glViewport (0, (h-static_cast<int>(w * height/width))/2, w, static_cast<int>(w * height/width));
	else
		glViewport ((w-static_cast<int>(h * width/height))/2, 0, static_cast<int>(h * width/height), h);
}

void DrawStepByStep::paintGL ()
{
	glClear (GL_COLOR_BUFFER_BIT);
	draw ();
}

void DrawStepByStep::drawSegment (const cbop::SweepEvent* se) const
{
	drawPoint (se->point);
	drawPoint (se->otherEvent->point);
}

void DrawStepByStep::draw ()
{
//	std::cout << "Start draw..." << std::endl;
	glMatrixMode (GL_MODELVIEW);
	glLoadIdentity ();
	glTranslatef (xoffset, yoffset, 0.0f);
	// show polygons
	glColor3f (1, 1, 1);
	for (Polygon::const_iterator i = subject.begin (); i != subject.end (); i++) {
		glBegin (GL_LINE_LOOP);
		for (Contour::const_iterator j = i->begin (); j != i->end (); j++)
			drawPoint (*j);
		glEnd ();
	}
	for (Polygon::const_iterator i = clipping.begin (); i != clipping.end (); i++) {
		glBegin (GL_LINE_LOOP);
		for (Contour::const_iterator j = i->begin (); j != i->end (); j++)
			drawPoint (*j);
		glEnd ();
	}
	glBegin (GL_LINES);
		// show sl events
		glColor3f (0, 1, 1);
		for (cbop::BooleanOpImp::const_sl_iterator it = boi->beginSL (); it != boi->endSL (); ++it)
			drawSegment (*it);
		// show sortedEvents
		glColor3f (1, 0.6, 0.6);
		for (cbop::BooleanOpImp::const_sortedEvents_iterator it = boi->beginSortedEvents (); it != boi->endSortedEvents (); ++it)
			if (((*it)->left && (*it)->inResult) || (!(*it)->left && (*it)->otherEvent->inResult))
				drawSegment (*it);
		// show out
		glColor3f (0, 0, 1);
		for (cbop::BooleanOpImp::const_out_iterator it = boi->beginOut (); it != boi->endOut (); ++it) {
			drawSegment (*it);
/*			if ((*it)->prevInside) {
				glColor3f (0, 1, 0);
				drawSegment ((*it)->prevInside);
				glColor3f (0, 0, 1);
			} */
		}
		if (boi->currentEvent ()) {
			// show edge associated to the sweep event
			glColor3f (1, 0, 0);
			drawSegment (boi->currentEvent ());
//			std::cout << "inOut: " << std::boolalpha << boi->currentEvent ()->inOut << std::endl;
//			std::cout << "otherInOut: " << std::boolalpha << boi->currentEvent ()->otherInOut << std::endl;
//			std::cout << boi->currentEvent ()->point << ' ' << std::boolalpha << boi->currentEvent ()->left << std::endl;
//			std::cout << boi->currentEvent ()->segment () << std::endl;
		}
	glEnd ();
	glEnable (GL_LINE_STIPPLE);
	glBegin (GL_LINES);
	// show previous event
/*		if (boi->currentEvent () && boi->currentEvent ()->prevInside) {
			glColor3f (1, 0, 1);
			glVertex2d ((boi->currentEvent ()->prevInside)->point.x (), boi->currentEvent ()->prevInside->point.y ());
			glVertex2d (boi->currentEvent ()->prevInside->otherEvent->point.x (), boi->currentEvent ()->prevInside->otherEvent->point.y ());
			textPrevious->setText (boi->currentEvent ()->prevInside->toString ().c_str ());
		}  else
			textPrevious->setText ("");*/
		if (boi->previousEvent ()) {
			glColor3f (1, 0, 1);
			drawSegment (boi->previousEvent ());
		}
	// show next event
		if (boi->nextEvent ()) {
			glColor3f (1, 1, 0);
			drawSegment (boi->nextEvent ());
		}
		// show sl
/*		glColor3f (0, 1, 0);
		glVertex2d (boi->currentPoint ().x (), boi->bb ().ymin ());
		glVertex2d (boi->currentPoint ().x (), boi->bb ().ymax ());*/
	glEnd ();
	glDisable (GL_LINE_STIPPLE);
	// show sweep event
	glColor3f (1, 0, 0);
	glBegin (GL_POINTS);
		drawPoint (boi->currentPoint ());
	glEnd ();
//	std::cout << "Finish draw..." << std::endl;
}

void DrawStepByStep::keyPressEvent (QKeyEvent* event)
{
	switch (event->key ()) {
		case Qt::Key_Z:
			if (zoom < 0.45)
				zoom += 0.05;
			glMatrixMode (GL_PROJECTION);
			glLoadIdentity ();
			gluOrtho2D (bb.xmin () + width*zoom, bb.xmax () - width*zoom, bb.ymin () + height*zoom, bb.ymax () - height*zoom);
			updateGL ();
			break;
		case Qt::Key_A:
			if (zoom > -0.45)
				zoom -= 0.05;
			glMatrixMode (GL_PROJECTION);
			glLoadIdentity ();
			gluOrtho2D (bb.xmin () + width*zoom, bb.xmax () - width*zoom, bb.ymin () + height*zoom, bb.ymax () - height*zoom);
			updateGL ();
			break;
		case Qt::Key_Left:
			xoffset -= 0.05*width;
			updateGL ();
			break;
		case Qt::Key_Right:
			xoffset += 0.05*width;
			updateGL ();
			break;
		case Qt::Key_Up:
			yoffset += 0.05*height;
			updateGL ();
			break;
		case Qt::Key_Down:
			yoffset -= 0.05*height;
			updateGL ();
			break;
	}
}

