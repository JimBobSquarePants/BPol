#ifndef STEPBYSTEPDIALOG_H
#define STEPBYSTEPDIALOG_H

#include <QDialog>
#include "../booleanop.h"

class QLineEdit;
class QPushButton;
class DrawStepByStep;
class bop::Polygon;
class QSemaphore;

class StepByStepDialog : public QDialog
{
	Q_OBJECT
public:
	StepByStepDialog (const bop::Polygon& subject, const bop::Polygon& clipping, bop::BooleanOpType op,
					  QWidget* parent = 0);

private:
	enum {SUBJECT, CLIPPING};
	QLineEdit* textNext;
	QLineEdit* textCurrent;
	QLineEdit* textPrevious;
	QPushButton* nextButton;
	DrawStepByStep* draw;
	bop::Polygon* result;
	bop::BooleanOpImp* boi;
	QSemaphore* doSomething;
	QSemaphore* somethingDone;

private slots:
	void nextStep ();
};

#endif
