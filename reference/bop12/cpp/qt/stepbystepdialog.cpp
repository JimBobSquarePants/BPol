#include <QtGui>

#include "stepbystepdialog.h"
#include "drawstepbystep.h"

StepByStepDialog::StepByStepDialog (const cbop::Polygon& subject, const cbop::Polygon& clipping, 
									cbop::BooleanOpType op, QWidget* parent) : QDialog (parent)
{
	textNext = new QLineEdit;
	textCurrent = new QLineEdit;
	textPrevious = new QLineEdit;
	textCurrent->setReadOnly (true);
	textPrevious->setReadOnly (true);
	textNext->setReadOnly (true);
	result = new cbop::Polygon;
	doSomething = new QSemaphore (0);
	somethingDone = new QSemaphore (0);
	boi = new cbop::BooleanOpImp (subject, clipping, *result, op, doSomething, somethingDone, true);
	boi->start ();
	draw = new DrawStepByStep (subject, clipping, boi, this);
	QVBoxLayout* leftLayout = new QVBoxLayout;
	leftLayout->addWidget (textNext);
	leftLayout->addWidget (textCurrent);
	leftLayout->addWidget (textPrevious);
	leftLayout->addWidget (draw);

	nextButton = new QPushButton (tr ("Next"));
	QVBoxLayout* rightLayout = new QVBoxLayout;
	rightLayout->addWidget (nextButton);
	rightLayout->addStretch ();
	
	QHBoxLayout* mainLayout = new QHBoxLayout;
	mainLayout->addLayout (leftLayout);
	mainLayout->addLayout (rightLayout);
	setLayout (mainLayout);
	setWindowTitle (tr ("Execute step by step"));
	connect (nextButton, SIGNAL (clicked ()), this, SLOT (nextStep ()));
}

void StepByStepDialog::nextStep ()
{
	if (boi->isFinished ())
		return;
	doSomething->release ();
	somethingDone->acquire ();
	textNext->setText (boi->nextEvent () ? boi->nextEvent ()->toString ().c_str () : "");
	textCurrent->setText (boi->currentEvent () ? boi->currentEvent ()->toString ().c_str () : "");
	textPrevious->setText (boi->previousEvent () ? boi->previousEvent ()->toString ().c_str () : "");
	draw->updateGL ();
}