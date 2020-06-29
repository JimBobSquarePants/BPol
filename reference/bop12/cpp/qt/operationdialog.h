#ifndef MYDIALOG_H
#define MYDIALOG_H

#include <QDialog>
#include <QButtonGroup>
#include "../booleanop.h"

class QRadioButton;
class QDialogButtonBox;

class OperationDialog : public QDialog
{
	Q_OBJECT
public:
	OperationDialog (QWidget* parent = 0);
	cbop::BooleanOpType operation () const { return option; }
	
private slots:
	void changeOption (int id) { option = cbop::BooleanOpType (id);}

private:
	QRadioButton* intersectionButton;
	QRadioButton* unionButton;
	QRadioButton* differenceButton;
	QRadioButton* xorButton;
	QButtonGroup* buttonGroup;
	QDialogButtonBox* buttonBox;
	cbop::BooleanOpType option;
};

#endif
