#pragma once

const char* raw_STL_O_Shell(R"(
solid Home
facet normal 0 1 0
outer loop
vertex -30 61 70
vertex -30 61 175
vertex 0 61 70
endloop
endfacet
facet normal 0 1 0
outer loop
vertex 0 61 70
vertex -30 61 175
vertex 0 61 175
endloop
endfacet
facet normal 0 1 0
outer loop
vertex 0 0 101.5
vertex 0 40 101.5
vertex 0 0 175
endloop
endfacet
facet normal 0 1 0
outer loop
vertex 0 0 175
vertex 0 40 101.5
vertex 0 61 175
endloop
endfacet
facet normal 0 1 0
outer loop
vertex 0 61 175
vertex 0 40 101.5
vertex 0 61 70
endloop
endfacet
facet normal 0 1 0
outer loop
vertex 0 61 70
vertex 0 40 101.5
vertex 0 40 0
endloop
endfacet
facet normal 1 0 0
outer loop
vertex 0 61 70
vertex 0 40 0
vertex 0 61 0
endloop
endfacet
facet normal 1 0 0
outer loop
vertex 0 61 175
vertex -30 61 175
vertex 0 0 175
endloop
endfacet
facet normal 1 0 0
outer loop
vertex 0 0 175
vertex -30 61 175
vertex -30 0 175
endloop
endfacet
facet normal 1 0 0
outer loop
vertex 0 61 70
vertex 0 61 0
vertex 30 61 70
endloop
endfacet
facet normal 1 0 0
outer loop
vertex 30 61 70
vertex 0 61 0
vertex 30 61 0
endloop
endfacet
facet normal 1 0 0
outer loop
vertex 70 61 70
vertex 70 100 70
vertex 30 61 70
endloop
endfacet
facet normal 1 0 0
outer loop
vertex 30 61 70
vertex 70 100 70
vertex -30 100 70
endloop
endfacet
facet normal 1 0 0
outer loop
vertex 30 61 70
vertex -30 100 70
vertex 0 61 70
endloop
endfacet
facet normal 1 0 0
outer loop
vertex 0 61 70
vertex -30 100 70
vertex -30 61 70
endloop
endfacet
facet normal 1 0 0
outer loop
vertex -30 100 70
vertex -30 100 0
vertex -30 61 70
endloop
endfacet
facet normal 1 0 0
outer loop
vertex -30 61 70
vertex -30 100 0
vertex -30 0 0
endloop
endfacet
facet normal 1 0 0
outer loop
vertex -30 61 70
vertex -30 0 0
vertex -30 0 175
endloop
endfacet
facet normal 1 0 0
outer loop
vertex -30 0 175
vertex -30 61 175
vertex -30 61 70
endloop
endfacet
facet normal 1 0 0
outer loop
vertex 70 100 70
vertex 70 100 0
vertex -30 100 70
endloop
endfacet
facet normal 1 0 0
outer loop
vertex -30 100 70
vertex 70 100 0
vertex -30 100 0
endloop
endfacet
facet normal 0 0 1
outer loop
vertex 70 61 87.5
vertex 30 61 87.5
vertex 70 40 87.5
endloop
endfacet
facet normal 0 0 1
outer loop
vertex 70 40 87.5
vertex 30 61 87.5
vertex 30 40 87.5
endloop
endfacet
facet normal 0 0 1
outer loop
vertex 30 61 87.5
vertex 30 61 70
vertex 30 40 87.5
endloop
endfacet
facet normal 0 0 1
outer loop
vertex 30 40 87.5
vertex 30 61 70
vertex 30 40 0
endloop
endfacet
facet normal 0 0 1
outer loop
vertex 30 40 0
vertex 30 61 70
vertex 30 61 0
endloop
endfacet
facet normal 0 0 1
outer loop
vertex 30 61 70
vertex 30 61 87.5
vertex 70 61 70
endloop
endfacet
facet normal 0 -1 0
outer loop
vertex 70 61 70
vertex 30 61 87.5
vertex 70 61 87.5
endloop
endfacet
facet normal 0 -1 0
outer loop
vertex 70 40 87.5
vertex 30 40 87.5
vertex 70 40 101.5
endloop
endfacet
facet normal 0 -1 0
outer loop
vertex 70 40 101.5
vertex 30 40 87.5
vertex 0 40 101.5
endloop
endfacet
facet normal 0 -1 0
outer loop
vertex 0 40 101.5
vertex 30 40 87.5
vertex 0 40 0
endloop
endfacet
facet normal 0 -1 0
outer loop
vertex 0 40 0
vertex 30 40 87.5
vertex 30 40 0
endloop
endfacet
facet normal 0 -1 0
outer loop
vertex 70 40 101.5
vertex 0 40 101.5
vertex 70 0 101.5
endloop
endfacet
facet normal 0 0 1
outer loop
vertex 70 0 101.5
vertex 0 40 101.5
vertex 0 0 101.5
endloop
endfacet
facet normal 0 0 1
outer loop
vertex -30 100 0
vertex 0 40 0
vertex -30 0 0
endloop
endfacet
facet normal 0 0 1
outer loop
vertex -30 0 0
vertex 0 40 0
vertex 30 40 0
endloop
endfacet
facet normal 0 0 1
outer loop
vertex -30 0 0
vertex 30 40 0
vertex 70 0 0
endloop
endfacet
facet normal 0 0 1
outer loop
vertex 70 0 0
vertex 30 40 0
vertex 70 100 0
endloop
endfacet
facet normal 0 0 1
outer loop
vertex 70 100 0
vertex 30 40 0
vertex 30 61 0
endloop
endfacet
facet normal 0 0 1
outer loop
vertex 70 100 0
vertex 30 61 0
vertex -30 100 0
endloop
endfacet
facet normal 0 0 1
outer loop
vertex -30 100 0
vertex 30 61 0
vertex 0 61 0
endloop
endfacet
facet normal 0 0 1
outer loop
vertex -30 100 0
vertex 0 61 0
vertex 0 40 0
endloop
endfacet
facet normal 0 0 1
outer loop
vertex -30 0 175
vertex -30 0 0
vertex 0 0 101.5
endloop
endfacet
facet normal 0 0 1
outer loop
vertex 0 0 101.5
vertex -30 0 0
vertex 70 0 0
endloop
endfacet
facet normal 0 0 1
outer loop
vertex 0 0 101.5
vertex 70 0 0
vertex 70 0 101.5
endloop
endfacet
facet normal -1 0 0
outer loop
vertex 0 0 101.5
vertex 0 0 175
vertex -30 0 175
endloop
endfacet
facet normal -1 0 0
outer loop
vertex 70 40 101.5
vertex 70 0 101.5
vertex 70 40 87.5
endloop
endfacet
facet normal -1 0 0
outer loop
vertex 70 40 87.5
vertex 70 0 101.5
vertex 70 0 0
endloop
endfacet
facet normal -1 0 0
outer loop
vertex 70 40 87.5
vertex 70 0 0
vertex 70 61 70
endloop
endfacet
facet normal -1 0 0
outer loop
vertex 70 61 70
vertex 70 0 0
vertex 70 100 0
endloop
endfacet
facet normal -1 0 0
outer loop
vertex 70 61 70
vertex 70 100 0
vertex 70 100 70
endloop
endfacet
facet normal -1 0 0
outer loop
vertex 70 61 70
vertex 70 61 87.5
vertex 70 40 87.5
endloop
endfacet
endsolid Home
)");

string STL_O_Shell(raw_STL_O_Shell);
