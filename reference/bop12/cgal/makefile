CC = g++
CXXFLAGS = -O2 -DCGAL_NDEBUG -frounding-math
LDFLAGS = -lm -lCGAL -lboost_thread -lgmp -lmpfr
TARGET = boolop
OBJS = polygon.o main.o booleanop.o

$(TARGET): $(OBJS)
	$(CC) -o $(TARGET) $(OBJS) $(LDFLAGS)

booleanop.o: booleanop.cpp booleanop.h polygon.h

main.o: main.cpp booleanop.h polygon.h

polygon.o: polygon.cpp polygon.h

clean:
	rm $(TARGET) $(OBJS) *~
