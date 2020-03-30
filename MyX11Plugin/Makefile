CXX=g++
CXXFLAGS=-O3 -fPIC
LDLIBS=-lX11 -lxdo
OBJ=MyX11Plugin.so
SRC=lib.cpp

all: build

build: $(SRC)
	$(CXX) $(SRC) $(CXXFLAGS) -shared -Bstatic $(LDLIBS) -o $(OBJ)

clean:
	rm -f $(OBJ)

