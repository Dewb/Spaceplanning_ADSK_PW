import unittest
import togo

class TestRect(unittest.TestCase):
    def test_extents(self):
        pos_1 = (20, 25)
        size_1 = (30, 50)
        rect_1 = togo.shapes.Rect(pos_1, size_1)
        self.assertEquals(rect_1.top, 25)
        self.assertEquals(rect_1.bottom, 25 + 50)
        self.assertEquals(rect_1.left, 20)
        self.assertEquals(rect_1.right, 20 + 30)

    def test_overlap(self):
        pos_1 = (0, 0)
        size_1 = (50, 50)
        rect_1 = togo.shapes.Rect(pos_1, size_1)
        pos_2 = (25, 25)
        size_2 = (50, 50)
        rect_2 = togo.shapes.Rect(pos_2, size_2)
        pos_3 = (51, 51)
        size_3 = (50, 50)
        rect_3 = togo.shapes.Rect(pos_3, size_3)
        self.assertTrue(rect_1.intersects(rect_2))
        self.assertTrue(rect_2.intersects(rect_1))
        self.assertFalse(rect_3.intersects(rect_1))

if __name__ == '__main__':
    unittest.main()
