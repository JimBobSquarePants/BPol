#ifndef STEPBYSTEPDIALOG_H
#define STEPBYSTEPDIALOG_H

#include <QDialog>
#include "../booleanop.h"

class QLineEdit;
class QPushButton;
class DrawStepByStep;
class Polygon;
class QSemaphore;

class StepByStepDialog : public QDialog
{
	Q_OBJECT
public:
	StepByStepDialog (const cbop::Polygon& subject, const cbop::Polygon& clipping, cbop::BooleanOpType op,
					  QWidget* parent = 0);

private:
	enum {SUBJECT, CLIPPING};
	QLineEdit* textNext;
	QLineEdit* textCurrent;
	QLineEdit* textPrevious;
	QPushButton* nextButton;
	DrawStepByStep* draw;
	cbop::Polygon* result;
	cbop::BooleanOpImp* boi;
	QSemaphore* doSomething;
	QSemaphore* somethingDone;

private slots:
	void nextStep ();
};

#endif
