#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include <QMainWindow>
#include <QAction>
#include <string>
#include "drawpolygons.h"

class cbop::Polygon;

class MainWindow : public QMainWindow
{
	Q_OBJECT
public:
	MainWindow (QWidget* parent = 0);

	void setSubject (const std::string& name);
	void setClipping (const std::string& name);
private slots:
	void openSubject ();
	void openClipping ();
	void setSubjectVisible () const { drawer->setVisible (DrawPolygons::SUBJECT, viewSubjectAction->isChecked ()); }
	void setClippingVisible () const { drawer->setVisible (DrawPolygons::CLIPPING, viewClippingAction->isChecked ()); }
	void setResultVisible () const { drawer->setVisible (DrawPolygons::RESULT, viewResultAction->isChecked ()); }
	void setWireframeVisible () const { drawer->setWireframe (viewWireframeAction->isChecked ()); }
	void computeBooleanOperation ();
	void executeStepByStep ();
private:
	void createActions ();
	void createMenus ();

	DrawPolygons* drawer;
	cbop::Polygon* subj;
	cbop::Polygon* clip;
	cbop::Polygon* result;
	QMenu* fileMenu;
	QAction* openSubjectAction;
	QAction* openClippingAction;
	QMenu* viewMenu;
	QAction* viewSubjectAction;
	QAction* viewClippingAction;
	QAction* viewResultAction;
	QAction* viewWireframeAction;
	QMenu* computeMenu;
	QAction* computeAction;
	QAction* stepByStepAction;
};

#endif
