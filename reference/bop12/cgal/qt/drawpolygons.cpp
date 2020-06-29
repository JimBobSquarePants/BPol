#include <GL/gl.h>
#include <QtOpenGL>
#include <iostream>
#include <algorithm>
#include "drawpolygons.h"

#ifndef CALLBACK
#define CALLBACK
#endif

void CALLBACK tessErrorCB (GLenum errno)
{
	std::cerr << "Tesselation error: " << gluErrorString (errno) << '\n';
}

void CALLBACK tessCombineCB (GLdouble coords[3], GLdouble* vertex_data[4], GLfloat weight[4], GLdouble** dataOut)
{
	*dataOut = new GLdouble[3];
	(*dataOut)[0] = coords[0];
	(*dataOut)[1] = coords[1];
	(*dataOut)[2] = coords[2];
}

DrawPolygons::DrawPolygons (Polygon* subj, Polygon* clip, Polygon* result, QWidget* parent) : 
                   QGLWidget (parent), viewWireframe (false), zoom (-0.05), xoffset (0), yoffset (0)
{
	setFocusPolicy (Qt::ClickFocus);
	polygons[SUBJECT] = subj;
	polygons[CLIPPING] = clip;
	polygons[RESULT] = result;
	std::fill_n (visible, 3, true);
}

void DrawPolygons::initializeGL ()
{
	typedef GLvoid (*parameterlessCallbackType)();
	tesselatorObj = gluNewTess ();
	gluTessCallback (tesselatorObj, GLU_TESS_BEGIN, (parameterlessCallbackType) glBegin);
	gluTessCallback (tesselatorObj, GLU_TESS_END, (parameterlessCallbackType) glEnd);
	gluTessCallback (tesselatorObj, GLU_TESS_VERTEX, (parameterlessCallbackType) glVertex3dv);
	gluTessCallback (tesselatorObj, GLU_TESS_ERROR, (parameterlessCallbackType) tessErrorCB);
	gluTessCallback (tesselatorObj, GLU_TESS_COMBINE, (parameterlessCallbackType) tessCombineCB);

	glClearColor (1, 1, 1, 1);
	glColor3f (0, 0, 0);
	glPointSize (5.0);
	glLineWidth (2.0f);
	glEnable (GL_BLEND);
	glLineStipple (1, 0x0FFF);
}

void DrawPolygons::resizeGL (int wi, int he)
{
	w = wi;
	h = he;
	if (polygons[SUBJECT]->ncontours () + polygons[CLIPPING]->ncontours () == 0)
		return;
	bb = polygons[SUBJECT]->bbox () + polygons[CLIPPING]->bbox ();
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

void DrawPolygons::paintGL ()
{
	glClear (GL_COLOR_BUFFER_BIT);
	draw ();
}

void DrawPolygons::draw ()
{
//	std::cout << "Start draw..." << std::endl;
	glMatrixMode (GL_MODELVIEW);
	glLoadIdentity ();
	glTranslatef (xoffset, yoffset, 0.0f);
	if (visible[SUBJECT] && polygons[SUBJECT]->ncontours () > 0) {
		glColor3f (0, 1, 0);
		drawPolygon (SUBJECT);
	}
	if (visible[CLIPPING] && polygons[CLIPPING]->ncontours () > 0) {
		glBlendFunc (GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
		glColor4f (1, 0, 0, 0.33);
		drawPolygon (CLIPPING);
	}
	if (visible[RESULT] && polygons[RESULT]->ncontours () > 0) {
		glColor3f (0, 0, 1);
		drawPolygon (RESULT);
	}

//	std::cout << "Finish draw..." << std::endl;
}

void DrawPolygons::drawPolygon (PolType pt)
{
	if (viewWireframe) {
		// render the vertices
		glBegin (GL_POINTS);
		for (Polygon::iterator i = polygons[pt]->begin (); i != polygons[pt]->end (); i++)
			for (Contour::iterator j = i->begin (); j != i->end (); j++)
				drawPoint (inexactPoint (*j));
		glEnd ();
		// render the edges
		for (Polygon::iterator i = polygons[pt]->begin (); i != polygons[pt]->end (); i++) {
			glBegin (GL_LINE_LOOP);
			for (Contour::iterator j = i->begin (); j != i->end (); j++)
				drawPoint (inexactPoint (*j));
			glEnd ();
		}
		return;
	}

	// the polygon must be rendered filled
	glCallList (displayList[pt]);
}

void DrawPolygons::drawFilledPolygon (PolType pt)
{
	int npoints = 0;
	for (Polygon::iterator i = polygons[pt]->begin (); i != polygons[pt]->end (); i++)
		npoints += i->nvertices ();
	GLdouble* vert = new GLdouble[npoints*3];
	int pp = 0; // number of points processed
	gluTessBeginPolygon (tesselatorObj, NULL);
	for (Polygon::iterator i = polygons[pt]->begin (); i != polygons[pt]->end (); i++) {
		gluTessBeginContour(tesselatorObj);
		for (Contour::iterator j = i->begin (); j != i->end (); j++) {
			IK::Point_2 inexact_pt = inexactPoint (*j);
			vert[pp++] = inexact_pt.x ();
			vert[pp++] = inexact_pt.y ();
			vert[pp++] = 0.0;
			gluTessVertex (tesselatorObj, &vert[pp-3], &vert[pp-3]);
		}
		gluTessEndContour (tesselatorObj);
	} 
	gluTessEndPolygon (tesselatorObj);
	delete[] vert;
}

void DrawPolygons::setPolygon (PolType pt)
{
	glDeleteLists (displayList[pt], 1);
	displayList[pt] = glGenLists (1);
	glNewList (displayList[pt], GL_COMPILE);
		drawFilledPolygon (pt);
	glEndList ();
	resizeGL (w, h);
	updateGL ();
}

void DrawPolygons::keyPressEvent (QKeyEvent* event)
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

