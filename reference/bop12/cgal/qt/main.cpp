#include <QApplication>
#include <QGLFormat>
#include <iostream>
#include "mainwindow.h"

int main (int argc, char* argv[])
{
	QApplication app (argc, argv);
	
	if (!QGLFormat::hasOpenGL ()) {
		std::cerr << "This system has no OpenGL support" << std::endl;
		return 1;
	}

	MainWindow* mainwindow = new MainWindow;
	mainwindow->resize (500, 500);
	mainwindow->show ();
	if (argc > 1)
		mainwindow->setSubject (argv[1]);
	if (argc > 2)
		mainwindow->setClipping (argv[2]);
	return app.exec ();
}
