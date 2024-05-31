using JellyCube;
using System;
using System.Linq;
using UnityEngine;


public class MoveV2: MonoBehaviour {
    [Header("Board")]
    [SerializeField] Renderer boardRenderer;
    [SerializeField] Transform boardCenter;
    float squareSize = 1f;
    [Header("Player")]
    [SerializeField] Transform player;
    public string playerTag = "Player";
    Transform[] otherCube;
    int[] otherCubePos;


    void Start() {
        //find other cube and add to array

        CheckOtherCubePos();
        //calculate map size
        if(boardRenderer != null) {
            Vector3 size = boardRenderer.bounds.size;
            float width = size.x;
            float length = size.z;
            Debug.Log("Width: " + Mathf.Round(width));
            Debug.Log("Length: " + Mathf.Round(length));
        }
    }

    void Update() {

        #region Check mouse click POS
        if(Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit)) {
                if(hit.collider.gameObject.CompareTag("Arena")) {
                    Debug.DrawLine(ray.origin, hit.point, Color.red, 2.0f);
                    Vector3 hitPoint = hit.point;
                    hitPoint.x = RoundToNearestHalf(hitPoint.x);
                    hitPoint.y = RoundToNearestHalf(hitPoint.y);
                    hitPoint.z = RoundToNearestHalf(hitPoint.z);
                    Debug.Log("Pos :" + GetSquareNumber(hitPoint) + "Raycast hit: " + hitPoint);

                    string path = PathFinding.FindPath(
                        PathFinding.GenerateMatrix(8, 8)
                        , otherCubePos, GetSquareNumber(
                            GetTrueCubePosition(player.transform)
                                ), GetSquareNumber(hitPoint));
                    Debug.Log(path);
                }
            }
            else {
                Debug.DrawRay(ray.origin, ray.direction * 100, Color.green, 2.0f);

            }
        }
        #endregion


    }

    void CheckOtherCubePos() {
        CubeController[] cubes = FindObjectsOfType<CubeController>();
        otherCube = cubes
                    .Where(cube => cube.gameObject.tag != playerTag)
                    .Select(cube => cube.transform)
                    .ToArray();
        otherCubePos = new int[otherCube.Length];

        for (int i = 0; i < otherCube.Length; i++) {
            otherCubePos[i] = GetSquareNumber(
                    GetTrueCubePosition(otherCube[i].transform)
                        );
        }
    }

    public int GetSquareNumber(Vector3 position) {
        Vector3 relativePosition = position - boardCenter.position;
        int col = Mathf.RoundToInt(relativePosition.x / squareSize + 3.5f);
        int row = Mathf.RoundToInt(3.5f - relativePosition.z / squareSize);
        if(col < 0 || col > 7 || row < 0 || row > 7) {
            Debug.LogError("Out of size");
            return -1;
        }
        int squareNumber = row * 8 + col + 1;
        return squareNumber;
    }

    float RoundToNearestHalf(float value) {
        return Mathf.Round(value * 2) / 2.0f;
    }

    //only use for cube object in this moment
    public Vector3 GetTrueCubePosition(Transform tf) {
        return tf.GetChild(0).position;
    }

    public class PathFinding {
        //static void Main(string[] args) {
        //    int[,] matrix = GenerateMatrix(8, 8); 
        //    int[] obstacles = { 9, 10, 11, 12, 13, 14, 15 }; 

        //    string path = FindPath(matrix, obstacles, 1, 64); 
        //    Console.WriteLine("Path: " + path);
        //}

        public static int[,] GenerateMatrix(int rows, int cols) {
            int[,] matrix = new int[rows, cols];
            int value = 1;
            for(int i = 0;i < rows;i++) {
                for(int j = 0;j < cols;j++) {
                    matrix[i, j] = value++;
                }
            }
            return matrix;
        }

        public static string FindPath(int[,] matrix, int[] obstacles, int startPoint, int endPoint) {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            int startRow = (startPoint - 1) / cols;
            int startCol = (startPoint - 1) % cols;
            int endRow = (endPoint - 1) / cols;
            int endCol = (endPoint - 1) % cols;

            string path = "";

            while(startRow != endRow || startCol != endCol) {
                if(startRow < endRow && !obstacles.Contains(matrix[startRow + 1, startCol])) {
                    path += "S ";
                    startRow++;
                    CubeManager.Instance.Move(new Vector3(0, 0, -1));
                    return path;
                }
                else if(startRow > endRow && !obstacles.Contains(matrix[startRow - 1, startCol])) {
                    path += "W ";
                    startRow--;
                    CubeManager.Instance.Move(new Vector3(0, 0, 1));
                    return path;
                }

                if(startCol < endCol && !obstacles.Contains(matrix[startRow, startCol + 1])) {
                    path += "D ";
                    startCol++;
                    CubeManager.Instance.Move(new Vector3(1, 0, 0));
                    return path;
                }
                else if(startCol > endCol && !obstacles.Contains(matrix[startRow, startCol - 1])) {
                    path += "A ";
                    startCol--;
                    CubeManager.Instance.Move(new Vector3(-1, 0, 0));
                    return path;
                }
            }

            return path.Trim();
        }
    }
}
