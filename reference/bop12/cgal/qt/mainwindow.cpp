#include <QtGui>

#include "mainwindow.h"
#include "drawpolygons.h"
#include "../booleanop.h"
#include "operationdialog.h"
#include "stepbystepdialog.h"

MainWindow::MainWindow (QWidget* parent) : QMainWindow (parent)
{
	subj = new Polygon;
	clip = new Polygon;
	result = new Polygon;
	drawer = new DrawPolygons (subj, clip, result);
	setCentralWidget (drawer);
	setWindowTitle (tr ("Boolean operations"));
	createActions ();
	createMenus ();
}

void MainWindow::createActions ()
{
	openSubjectAction = new QAction (tr ("Open &subject polygon"), this);
	openClippingAction = new QAction (tr ("Open &clipping polygon"), this);
	viewSubjectAction = new QAction (tr ("&Subject"), this);
	viewSubjectAction->setCheckable (true);
	viewSubjectAction->setChecked (true);
	viewClippingAction = new QAction (tr ("&Clipping"), this);
	viewClippingAction->setCheckable (true);
	viewClippingAction->setChecked (true);
	viewResultAction = new QAction (tr ("&Result"), this);
	viewResultAction->setCheckable (true);
	viewResultAction->setChecked (true);
	viewWireframeAction = new QAction (tr ("&Wireframe"), this);
	viewWireframeAction->setCheckable (true);
	viewWireframeAction->setChecked (false);
	computeAction = new QAction (tr ("&Boolean operation"), this);
	stepByStepAction = new QAction (tr ("&Step by step"), this);
}

void MainWindow::createMenus ()
{
	fileMenu = menuBar ()->addMenu (tr ("&File"));
	fileMenu->addAction (openSubjectAction);
	connect (openSubjectAction, SIGNAL (triggered ()), this, SLOT (openSubject ()));
	fileMenu->addAction (openClippingAction);
	connect (openClippingAction, SIGNAL (triggered ()), this, SLOT (openClipping ()));
	viewMenu = menuBar ()->addMenu (tr ("&View"));
	viewMenu->addAction (viewSubjectAction);
	connect (viewSubjectAction, SIGNAL (triggered ()), this, SLOT (setSubjectVisible ()));
	viewMenu->addAction (viewClippingAction);
	connect (viewClippingAction, SIGNAL (triggered ()), this, SLOT (setClippingVisible ()));
	viewMenu->addAction (viewResultAction);
	connect (viewResultAction, SIGNAL (triggered ()), this, SLOT (setResultVisible ()));
	viewMenu->addAction (viewWireframeAction);
	connect (viewWireframeAction, SIGNAL (triggered ()), this, SLOT (setWireframeVisible ()));
	computeMenu = menuBar ()->addMenu (tr ("&Compute"));
	computeMenu->addAction (computeAction);
	connect (computeAction, SIGNAL (triggered ()), this, SLOT (computeBooleanOperation ()));
	computeMenu->addAction (stepByStepAction);
	connect (stepByStepAction, SIGNAL (triggered ()), this, SLOT (executeStepByStep ()));
}

void MainWindow::setSubject (const std::string& name)
{
	subj->open (name);
	result->clear ();
	drawer->setPolygon (DrawPolygons::SUBJECT);}

void MainWindow::setClipping (const std::string& name)
{
	clip->open (name);
	result->clear ();
	drawer->setPolygon (DrawPolygons::CLIPPING);
}

void MainWindow::openSubject ()
{
	QString title = "Open subject polygon";
	QString fileName = QFileDialog::getOpenFileName (this, title, "/home/fmartin/src/Qt4/boolean/polygons");
	if (!fileName.isEmpty ())
		setSubject (fileName.toStdString ());
}

void MainWindow::openClipping ()
{
	QString title = "Open clipping polygon";
	QString fileName = QFileDialog::getOpenFileName (this, title, "/home/fmartin/src/Qt4/boolean/polygons");
	if (!fileName.isEmpty ())
		setClipping (fileName.toStdString ());
}

void MainWindow::computeBooleanOperation ()
{
	OperationDialog dialog (this);
	if (dialog.exec ()) {
		result->clear ();
		bop::compute (*subj, *clip, *result, dialog.operation ());
		drawer->setPolygon (DrawPolygons::RESULT);
	}
}

void MainWindow::executeStepByStep ()
{
	OperationDialog dialogOp (this);
	if (dialogOp.exec ()) {
		StepByStepDialog dialog (*subj, *clip, dialogOp.operation (), this);
		dialog.resize (500, 500);
		dialog.exec ();
	}
}
