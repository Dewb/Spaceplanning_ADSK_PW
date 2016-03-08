function intersection(cir0, cir1)
{
    var x0 = cir0.translation.x;
    var y0 = cir0.translation.y;
    var r0 = cir0.vertices[0].length();
    var x1 = cir1.translation.x;
    var y1 = cir1.translation.y;
    var r1 = cir1.vertices[0].length();

    var a, dx, dy, d, h, rx, ry;
    var x2, y2;

    /* dx and dy are the vertical and horizontal distances between
     * the circle centers.
     */
    dx = x1 - x0;
    dy = y1 - y0;

    /* Determine the straight-line distance between the centers. */
    d = Math.sqrt((dy*dy) + (dx*dx));

    /* Check for solvability. */
    if (d > (r0 + r1)) 
    {
        /* no solution. circles do not intersect. */
        return false;
    }
    if (d < Math.abs(r0 - r1)) 
    {
        /* no solution. one circle is contained in the other */
        return false;
    }

    /* 'point 2' is the point where the line through the circle
     * intersection points crosses the line between the circle
     * centers.  
     */

    /* Determine the distance from point 0 to point 2. */
    a = ((r0*r0) - (r1*r1) + (d*d)) / (2.0 * d) ;

    /* Determine the coordinates of point 2. */
    x2 = x0 + (dx * a/d);
    y2 = y0 + (dy * a/d);

    /* Determine the distance from point 2 to either of the
     * intersection points.
     */
    h = Math.sqrt((r0*r0) - (a*a));

    /* Now determine the offsets of the intersection points from
     * point 2.
     */
    rx = -dy * (h/d);
    ry = dx * (h/d);

    /* Determine the absolute intersection points. */
    var xi = x2 + rx;
    var xi_prime = x2 - rx;
    var yi = y2 + ry;
    var yi_prime = y2 - ry;

    return [[xi, yi], [xi_prime, yi_prime]];
}

function edist(p1, p2)
{
    var sum_of_squares = 0;
    sum_of_squares += Math.pow(p2[0] - p1[0], 2);
    sum_of_squares += Math.pow(p2[1] - p1[1], 2);
    var result = [Math.sqrt(sum_of_squares), p2];
    return result;
}

function get_closest_point(p1, plist)
{
    function compare_func(x, y) { return x[0] - y[0]; }
    function partial_edist(p2) { return edist(p1, p2); }
    var results = plist.map(partial_edist);
    results.sort(compare_func);
    console.log(results)
    return results[0][1];
}

var unique = function(a) 
{
    return a.reduce(function(p, c)
    {
        if (p.indexOf(c) < 0) p.push(c);
            return p;
    }, []);
};

function sort_points(points)
{
    points = unique(points);
    var results = [];
    var pt = points.pop();
    results.push(pt);
    while(points.length)
    {
        pt = get_closest_point(pt, points);
        results.push(pt);
        points.splice(points.indexOf(pt), 1);
    }
    return results;
}

function draw_circles()
{
    // Make an instance of two and place it on the page.
    var elem = document.getElementById('draw-shapes').children[0];
    var params = { width: 600, height: 600 };
    var two = new Two(params).appendTo(elem);

    function Circle(x, y, r)
    {
        this.x = x;
        this.y = y;
        this.r = r;
        this._circle = two.makeCircle(x, y, r);
    };
    Circle.inherits(two.Polygon);

    // two has convenience methods to create shapes.
    var circles = [];
    var lines = [];
    var idx;
    var deg = Math.PI / 180;
    var xc = params.width / 2.0;
    var yc = params.height / 2.0;
    var r = yc / 2.0;
    var x;
    var y;

    var grp = two.makeGroup();
    var points = [];

    for (idx = 0; idx <= 6; idx += 1)
    {
        if(idx == 0)
        {
            //circles[idx] = two.makeCircle(xc, yc, r);
            circles[idx] = Circle(xc, yc, r);
        } else
        {
            x = r * Math.cos(60 * idx * deg) + xc;
            y = r * Math.sin(60 * idx * deg) + yc;
            circles[idx] = two.makeCircle(x, y, r);
            grp.add(circles[idx]);
            points.push.apply(points, intersection(circles[idx], circles[0]));
        }
        circles[idx].stroke = 'orangered'; // Accepts all valid css color
        circles[idx].linewidth = 1;
        circles[idx].noFill();
    }
    points = sort_points(points);
    for (var idx = 0; idx < points.length; idx++)
    {
        var idx2 = (idx + 1) % points.length;
        console.log(idx + " " + idx2);
        var pts = points[idx].concat(points[idx2]);
        lines[idx] = two.makeLine.apply(two, pts);
        grp.add(lines[idx]);
    }
    //grp.mask = circles[0];

    // Don't forget to tell two to render everything
    // to the screen
    two.update();
}
