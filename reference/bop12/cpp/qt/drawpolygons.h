#ifndef DRAWPOLYGONS_H
#define DRAWPOLYGONS_H

#include <QGLWidget>
#include <GL/gl.h>
#include <vector>
#include <string>
#include <iostream>
#include "../polygon.h"

class DrawPolygons : public QGLWidget
{
	Q_OBJECT
public:
	enum PolType { SUBJECT, CLIPPING, RESULT };
	DrawPolygons (cbop::Polygon* subj, cbop::Polygon* clip, cbop::Polygon* result, QWidget* parent = 0);
	
	void setPolygon (PolType pt);
	void setVisible (PolType pt, bool b) { visible[pt] = b; updateGL (); }
	void setWireframe (bool b) { viewWireframe = b; updateGL (); }
protected:
	void initializeGL ();
	void resizeGL (int width, int height);
	void paintGL ();
	void keyPressEvent (QKeyEvent *e);

private:
	void drawPolygon (PolType pt);
	void drawFilledPolygon (PolType pt);
	void drawPoint (const cbop::Point_2& p) const { glVertex2d (p.x (), p.y ()); }
	cbop::Polygon* polygons[3];
	bool visible[3];
	bool viewWireframe;
	void draw ();
	GLdouble zoom;
	GLdouble xoffset, yoffset;
	GLdouble width, height;
	cbop::Bbox_2 bb;
	int w, h; // window size
	GLuint displayList[3];
	GLUtesselator* tesselatorObj;
};

#endif
