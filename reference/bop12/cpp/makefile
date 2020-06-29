CC = g++
CXXFLAGS = -O3 -ansi
LDFLAGS = -lm
TARGET = boolop
OBJS = polygon.o utilities.o main.o booleanop.o

$(TARGET): $(OBJS)
	$(CC) -o $(TARGET) $(OBJS) $(LDFLAGS)

booleanop.o: booleanop.cpp booleanop.h polygon.h utilities.h point_2.h bbox_2.h segment_2.h

main.o: main.cpp booleanop.h polygon.h utilities.h point_2.h bbox_2.h segment_2.h

polygon.o: polygon.cpp polygon.h utilities.h point_2.h bbox_2.h segment_2.h

utilities.o: utilities.cpp utilities.h point_2.h bbox_2.h segment_2.h

clean:
	rm $(TARGET) $(OBJS) *~
