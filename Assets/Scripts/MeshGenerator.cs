﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public SquareGrid grid;
    List<Vector3> verticies;
    List<int> triangles;
    public void GenerateMesh(int[,] map, float squareSize) {
        verticies = new List<Vector3>();
        triangles = new List<int>();
        grid = new SquareGrid(map, squareSize);
        for (int x = 0; x < grid.squares.GetLength(0); x++) {
            for (int y = 0; y < grid.squares.GetLength(1); y++) {
                TrangulateSquare(grid.squares[x, y]);
            }
        }
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = verticies.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    void TrangulateSquare(Square square) {
        switch (square.configuration) {
        case 0:
            break;
        case 1:
            MeshFromPoints(square.centerBottom, square.bottomLeft, square.centerLeft);
            break;
        case 2:
            MeshFromPoints(square.centerRight, square.bottomRight, square.centerBottom);
            break;
        case 4:
            MeshFromPoints(square.centerTop, square.topRight, square.centerRight);
            break;
        case 8:
            MeshFromPoints(square.topLeft, square.centerTop, square.centerLeft);
            break;

        case 3:
            MeshFromPoints(square.centerRight, square.bottomRight, square.bottomLeft, square.centerLeft);
            break;
        case 6:
            MeshFromPoints(square.centerTop, square.topRight, square.bottomRight, square.centerBottom);
            break;
        case 9:
            MeshFromPoints(square.topLeft, square.centerTop, square.centerBottom, square.bottomLeft);
            break;
        case 12:
            MeshFromPoints(square.topLeft, square.topRight, square.centerRight, square.centerLeft);
            break;
        case 5:
            MeshFromPoints(square.centerTop, square.topRight, square.centerRight, square.centerBottom, square.bottomLeft, square.centerLeft);
            break;
        case 10:
            MeshFromPoints(square.topLeft, square.centerTop, square.centerRight, square.bottomRight, square.centerBottom, square.centerLeft);
            break;

        case 7:
            MeshFromPoints(square.centerTop, square.topRight, square.bottomRight, square.bottomLeft, square.centerLeft);
            break;
        case 11:
            MeshFromPoints(square.topLeft, square.centerTop, square.centerRight, square.bottomRight, square.bottomLeft);
            break;
        case 13:
            MeshFromPoints(square.topLeft, square.topRight, square.centerRight, square.centerBottom, square.bottomLeft);
            break;
        case 14:
            MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centerBottom, square.centerLeft);
            break;

        case 15:
            MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
            break;
        }
    }

    void MeshFromPoints(params Node[] points) {
        AssignVerticies(points);
        if (points.Length >= 3) 
            CreateTriangle(points[0], points[1], points[2]);
        if (points.Length >= 4) 
            CreateTriangle(points[0], points[2], points[3]);
        if (points.Length >= 5) 
            CreateTriangle(points[0], points[3], points[4]);
        if (points.Length >= 6) 
            CreateTriangle(points[0], points[4], points[5]);
    }

    void AssignVerticies(Node[] points) {
        for (int i = 0; i < points.Length; i++) {
            if (points[i].vertexIndex == -1) {
                points[i].vertexIndex = verticies.Count;
                verticies.Add(points[i].position);
            }
        }
    }

    void CreateTriangle(Node a, Node b, Node c) {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);
    }

    void OnDrawGizmos() {
        return;
        if (grid != null) {
            for (int x = 0; x < grid.squares.GetLength(0); x++) {
                for (int y = 0; y < grid.squares.GetLength(1); y++) {
                    Gizmos.color = (grid.squares[x, y].topLeft.active) ? Color.black: Color.white;
                    Gizmos.DrawCube(grid.squares[x, y].topLeft.position, Vector3.one * 0.4f);
                    Gizmos.color = (grid.squares[x, y].topRight.active) ? Color.black: Color.white;
                    Gizmos.DrawCube(grid.squares[x, y].topRight.position, Vector3.one * 0.4f);
                    Gizmos.color = (grid.squares[x, y].bottomLeft.active) ? Color.black: Color.white;
                    Gizmos.DrawCube(grid.squares[x, y].bottomLeft.position, Vector3.one * 0.4f);
                    Gizmos.color = (grid.squares[x, y].bottomRight.active) ? Color.black: Color.white;
                    Gizmos.DrawCube(grid.squares[x, y].bottomRight.position, Vector3.one * 0.4f);

                    Gizmos.color = Color.gray;
                    Gizmos.DrawCube(grid.squares[x, y].centerLeft.position, Vector3.one * 0.2f);
                    Gizmos.DrawCube(grid.squares[x, y].centerTop.position, Vector3.one * 0.2f);
                    Gizmos.DrawCube(grid.squares[x, y].centerBottom.position, Vector3.one * 0.2f);
                    Gizmos.DrawCube(grid.squares[x, y].centerRight.position, Vector3.one * 0.2f);
                }
            }
        }
    }

    public class SquareGrid {
        public Square[,] squares;
        public SquareGrid(int[,] map, float squareSize) {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

            for (int x = 0; x < nodeCountX; x++) {
                for (int y = 0; y < nodeCountY; y++) {
                    Vector3 pos = new Vector3(-mapWidth/2 + x * squareSize + squareSize / 2, 0, -mapHeight / 2 + y * squareSize + squareSize/2);
                    controlNodes[x, y] = new ControlNode(pos, map[x, y] == 1, squareSize);
                }
            }
            squares = new Square[nodeCountX - 1, nodeCountY - 1];
            for (int x = 0; x < nodeCountX - 1; x++) {
                for (int y = 0; y < nodeCountY - 1; y++) {
                    squares[x, y] = new Square(controlNodes[x, y+1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
                }
            }
        }
    }
    public class Square {
        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node centerTop, centerRight, centerBottom, centerLeft;
        public int configuration;
        public Square(ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft) {
            topLeft = _topLeft;
            topRight = _topRight;
            bottomLeft = _bottomLeft;
            bottomRight = _bottomRight;

            centerTop = topLeft.right;
            centerRight = bottomRight.above;
            centerBottom = bottomLeft.right;
            centerLeft = bottomLeft.above;
            
            if (topLeft.active)
                configuration += 8;
            if (topRight.active)
                configuration += 4;
            if (bottomRight.active)
                configuration += 2;
            if (bottomLeft.active)
                configuration += 1;
        }
    }
    public class Node {
        public Vector3 position;
        public int vertexIndex = -1;

        public Node(Vector3 _pos) {
            position = _pos;
        }
    }

    public class ControlNode: Node {
        public bool active;
        public Node above, right;
        public ControlNode(Vector3 _pos, bool _active, float squareSize) : base(_pos) {
            active = _active;
            above = new Node(position + Vector3.forward * squareSize / 2f);
            right = new Node(position + Vector3.right * squareSize / 2f);
        }
    }
}
