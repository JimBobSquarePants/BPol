######################################################################
# Automatically generated by qmake (2.01a) mar ene 31 10:09:24 2012
######################################################################

QT += opengl
LIBS += -lCGAL
LIBS += -lboost_thread
LIBS += -lgmp
LIBS += -lboost_thread
LIBS += -lmpfr
QMAKE_CXXFLAGS += -frounding-math -D__STEPBYSTEP
TEMPLATE = app
TARGET = 
DEPENDPATH += .
INCLUDEPATH += .

# Input
HEADERS += ../booleanop.h \
           drawpolygons.h \
           drawstepbystep.h \
           mainwindow.h \
           operationdialog.h \
           ../polygon.h \
           stepbystepdialog.h
SOURCES += ../booleanop.cpp \
           drawpolygons.cpp \
           drawstepbystep.cpp \
           main.cpp \
           mainwindow.cpp \
           operationdialog.cpp \
           ../polygon.cpp \
           stepbystepdialog.cpp