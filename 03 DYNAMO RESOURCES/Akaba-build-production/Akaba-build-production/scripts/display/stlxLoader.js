STLxLoader = function() {};

STLxLoader.prototype = {

  constructor: STLxLoader,

  parseASCIIOutlines: function(data) {
    var polygons = [];

    var result;
    var patternOutline = /outline([\s\S]*?)endoutline/g;
    while ((result = patternOutline.exec(data)) !== null) {
      var outlineText = result[0];
      var patternHeight = /height[\s]+([\-+]?[0-9]+\.?[0-9]*([eE][\-+]?[0-9]+)?)/g;
      var height = 0;
      if ((result = patternHeight.exec(outlineText)) !== null)
        height = parseFloat(result[1]);

      var patternPolygon = /polygon([\s\S]*?)polygon/g;
      while ((result = patternPolygon.exec(outlineText)) !== null) {
        var polygon = [];
        var polygonText = result[0];
        var patternVertex = /vertex[\s]+([\-+]?[0-9]+\.?[0-9]*([eE][\-+]?[0-9]+)?)+[\s]+([\-+]?[0-9]*\.?[0-9]+([eE][\-+]?[0-9]+)?)+/g;
        while ((result = patternVertex.exec(polygonText)) !== null)
          polygon.push({ 
            x: parseFloat(result[1]), 
            y: parseFloat(result[3]), 
            z: height});
        if (polygon.length > 1) {
          polygon.push(polygon[0]);
          polygons.push(polygon);
        }
      }
    }

    return polygons;
  },

  parseASCIIGrids: function(data) {
    var grids = [];

    var result;
    var patternGrid = /grid([\s\S]*?)endgrid/g;
    while ((result = patternGrid.exec(data)) !== null) {
      var gridText = result[0];
      var patternHeight = /height[\s]+([\-+]?[0-9]+\.?[0-9]*([eE][\-+]?[0-9]+)?)/g;
      var height = 0;
      if ((result = patternHeight.exec(gridText)) !== null)
        height = parseFloat(result[1]);

      var basis = [];
      var patternBasis = /basis([\s\S]*?)basis/g;
      if ((result = patternBasis.exec(gridText)) === null)
        continue;
      var basisText = result[0];
      var patternOrdinal = /ordinal[\s]+([\-+]?[0-9]+\.?[0-9]*([eE][\-+]?[0-9]+)?)+[\s]+([\-+]?[0-9]*\.?[0-9]+([eE][\-+]?[0-9]+)?)+[\s]+([\-+]?[0-9]*\.?[0-9]+([eE][\-+]?[0-9]+)?)+/g;
      while ((result = patternOrdinal.exec(basisText)) !== null) {
        basis.push({ 
          gridSize: parseFloat(result[1]), 
          x: parseFloat(result[3]), 
          y: parseFloat(result[5])
        });
      }

      var axes = [];
      var patternAxes = /axes([\s\S]*?)endaxes/g;
      if ((result = patternAxes.exec(gridText)) === null)
        continue;
      var axesText = result[0];
      var patternAxis = /axis([\s\S]*?)endaxis/g;
      while ((result = patternAxis.exec(gridText)) !== null) {
        var axis = [];
        var axisText = result[0];
        var patternSeg = /seg[\s]+([\-+]?[0-9]+\.?[0-9]*([eE][\-+]?[0-9]+)?)+[\s]+([\-+]?[0-9]*\.?[0-9]+([eE][\-+]?[0-9]+)?)+[\s]+([\-+]?[0-9]*\.?[0-9]+([eE][\-+]?[0-9]+)?)+/g;
        while ((result = patternSeg.exec(axisText)) !== null) {
          axis.push({ 
            coord: parseInt(result[1]), 
            v0: parseFloat(result[3]), 
            v1: parseFloat(result[5])
          });
        }
        axes.push(axis);
      }

      grids.push({
        height: height,
        basis: basis,
        axes: axes
      });
    }

    return grids;
  }
}
