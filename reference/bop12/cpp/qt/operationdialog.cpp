#include <QtGui>

#include "operationdialog.h"

OperationDialog::OperationDialog (QWidget* parent) : QDialog (parent)
{
	intersectionButton = new QRadioButton (tr ("&Intersection"));
	unionButton = new QRadioButton (tr ("&Union"));
	differenceButton = new QRadioButton (tr ("&Difference"));
	xorButton = new QRadioButton (tr ("&XOR"));
	intersectionButton->setChecked (true);
	option = cbop::INTERSECTION;
	
	QVBoxLayout* verticalLayout = new QVBoxLayout;
	verticalLayout->addWidget (intersectionButton);
	verticalLayout->addWidget (unionButton);
	verticalLayout->addWidget (differenceButton);
	verticalLayout->addWidget (xorButton);
	buttonGroup = new QButtonGroup;
	buttonGroup->addButton (intersectionButton, cbop::INTERSECTION);
	buttonGroup->addButton (unionButton, cbop::UNION);
	buttonGroup->addButton (differenceButton, cbop::DIFFERENCE);
	buttonGroup->addButton (xorButton, cbop::XOR);
	
	buttonBox = new QDialogButtonBox (QDialogButtonBox::Ok | QDialogButtonBox::Cancel);
	verticalLayout->addWidget (buttonBox);
	setLayout (verticalLayout); 
	setWindowTitle (tr ("Select Boolean operation"));
	setFixedHeight (sizeHint ().height ());

	connect (buttonGroup, SIGNAL (buttonClicked (int)), this, SLOT ( changeOption (int)));
	connect (buttonBox, SIGNAL (rejected ()), this, SLOT ( reject ()));
	connect (buttonBox, SIGNAL (accepted ()), this, SLOT ( accept ()));
}
