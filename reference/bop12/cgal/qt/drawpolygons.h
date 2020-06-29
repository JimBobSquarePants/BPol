#ifndef DRAWPOLYGONS_H
#define DRAWPOLYGONS_H

#include <QGLWidget>
#include <GL/gl.h>
#include <vector>
#include <string>
#include <iostream>
#include "../polygon.h"
#include <CGAL/Simple_cartesian.h>
#include <CGAL/Cartesian_converter.h>

typedef CGAL::Exact_predicates_exact_constructions_kernel EK;
typedef CGAL::Simple_cartesian<double>                    IK;
typedef CGAL::Cartesian_converter<EK,IK>                  EK_to_IK;

using namespace bop;

class DrawPolygons : public QGLWidget
{
	Q_OBJECT
public:
	enum PolType { SUBJECT, CLIPPING, RESULT };
	DrawPolygons (Polygon* subj, Polygon* clip, Polygon* result, QWidget* parent = 0);
	
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
	IK::Point_2 inexactPoint (const IK::Point_2& p) const { return p; }
	IK::Point_2 inexactPoint (const EK::Point_2& p) const { EK_to_IK toInexact; return toInexact (p); }
	void drawPoint (const IK::Point_2& p) const { glVertex2d (p.x (), p.y ()); }
	Polygon* polygons[3];
	bool visible[3];
	bool viewWireframe;
	void draw ();
	GLdouble zoom;
	GLdouble xoffset, yoffset;
	GLdouble width, height;
	CGAL::Bbox_2 bb;
	int w, h; // window size
	GLuint displayList[3];
	GLUtesselator* tesselatorObj;
};

#endif
