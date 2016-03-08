#pragma once

const char* raw_STL_R_Shell(R"(
solid Home
facet normal 0 0 1
outer loop
vertex 70 40 70
vertex 0 40 70
vertex 70 0 70
endloop
endfacet
facet normal 0 0 1
outer loop
vertex 70 0 70
vertex 0 40 70
vertex 0 0 70
endloop
endfacet
facet normal 0 0 1
outer loop
vertex 70 0 0
vertex 0 0 0
vertex 70 40 0
endloop
endfacet
facet normal 0 0 1
outer loop
vertex 70 40 0
vertex 0 0 0
vertex 0 40 0
endloop
endfacet
facet normal 0 0 1
outer loop
vertex 0 0 70
vertex 0 0 0
vertex 70 0 70
endloop
endfacet
facet normal 0 0 1
outer loop
vertex 70 0 70
vertex 0 0 0
vertex 70 0 0
endloop
endfacet
facet normal 0 0 -1
outer loop
vertex 0 40 70
vertex 0 40 0
vertex 0 0 70
endloop
endfacet
facet normal 0 0 -1
outer loop
vertex 0 0 70
vertex 0 40 0
vertex 0 0 0
endloop
endfacet
facet normal 0 0 -1
outer loop
vertex 70 40 70
vertex 70 40 0
vertex 0 40 70
endloop
endfacet
facet normal 0 0 -1
outer loop
vertex 0 40 70
vertex 70 40 0
vertex 0 40 0
endloop
endfacet
facet normal 0 0 -1
outer loop
vertex 70 0 70
vertex 70 0 0
vertex 70 40 70
endloop
endfacet
facet normal 0 0 -1
outer loop
vertex 70 40 70
vertex 70 0 0
vertex 70 40 0
endloop
endfacet
endsolid Home
)");

string STL_R_Shell(raw_STL_R_Shell);
