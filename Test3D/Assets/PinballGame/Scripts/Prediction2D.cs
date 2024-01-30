using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Prediction2D : Singleton<Prediction2D>
{
    public DropMachine dropMachine;
    public GameObject obstacles;
    public int maxIterations;

    Scene currentScene;
    Scene predictionScene;

    PhysicsScene2D currentPhysicsScene;
    PhysicsScene2D predictionPhysicsScene;

    List<GameObject> dummyObstacles = new List<GameObject>();

    LineRenderer lineRenderer;
    [HideInInspector] public GameObject dummyBall;
    [HideInInspector] public GameObject newDummyBall;

    public int curIterationIndex = 0;
    public List<Vector3> linePositions = new List<Vector3>(); //x, y ÁÂÇ¥ z°ªÀº rotation°ª

    void Start()
    {
        Init();
        copyAllObstacles();
    }

    public void ClearLineRenderer()
    {
        lineRenderer.positionCount = 0;
        lineRenderer.SetPositions(new Vector3[0]);
    }

    public void Init()
    {
        currentScene = SceneManager.GetActiveScene();
        currentPhysicsScene = currentScene.GetPhysicsScene2D();


        CreateSceneParameters parameters = new CreateSceneParameters(LocalPhysicsMode.Physics2D);
        predictionScene = SceneManager.CreateScene("Prediction2D", parameters);
        predictionPhysicsScene = predictionScene.GetPhysicsScene2D();

        lineRenderer = GetComponent<LineRenderer>();

        if (dummyBall == null)
        {
            dummyBall = Instantiate(dropMachine.shooter.ballPrefab);
            SceneManager.MoveGameObjectToScene(dummyBall, predictionScene);
        }
    }

    //void FixedUpdate()
    //{
    //    if (currentPhysicsScene.IsValid() && !dropMachine.isTest)
    //    {
    //        currentPhysicsScene.Simulate(Time.fixedDeltaTime);
    //    }
    //}

    public void copyAllObstacles()
    {
        foreach (Transform t in obstacles.transform)
        {
            GameObject fakeT = Instantiate(t.gameObject);
            fakeT.transform.position = t.position;
            fakeT.transform.rotation = t.rotation;
            Renderer fakeR = fakeT.GetComponent<Renderer>();
            if (fakeR)
            {
                fakeR.enabled = false;
            }
            SceneManager.MoveGameObjectToScene(fakeT, predictionScene);
            dummyObstacles.Add(fakeT);
        }
    }

    void killAllObstacles()
    {
        foreach (var o in dummyObstacles)
        {
            Destroy(o);
        }
        dummyObstacles.Clear();
    }

    public void predict(GameObject ball, Vector3 currentPosition, Vector2 force)
    {
        if (currentPhysicsScene.IsValid() && predictionPhysicsScene.IsValid())
        {
            if (newDummyBall == null)
            {
                newDummyBall = Instantiate(dummyBall);
            }
            curIterationIndex = 0;
            linePositions.Clear();
            SceneManager.MoveGameObjectToScene(newDummyBall, predictionScene);
            newDummyBall.SetActive(true);
            newDummyBall.transform.position = currentPosition;
            newDummyBall.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
            //var dummyBallRigid = newDummyBall.GetComponent<CircleCollider2D>();
            lineRenderer.positionCount = 0;
            lineRenderer.SetPositions(new Vector3[0]);
            lineRenderer.positionCount = maxIterations;

            for (int i = 0; i < maxIterations; i++)
            {
                curIterationIndex = i;
                predictionPhysicsScene.Simulate(0.03f);
                lineRenderer.SetPosition(i, newDummyBall.transform.position);
                linePositions.Add(newDummyBall.transform.position);
                if (!newDummyBall.activeSelf)
                {
                    lineRenderer.positionCount = i;
                    break;
                }
            }

            if(lineRenderer.positionCount >= maxIterations)
            {
                dropMachine.CheckMaxIteration();
            }

            //var finIndex = CheckLineBox();
            //
            //if(finIndex < 0)
            //{
            //    dropMachine.CheckMaxIteration();
            //}
            ReturnDummyBall(newDummyBall);
        }
    }

    public void ReturnDummyBall(GameObject newDummyBall)
    {
        if (newDummyBall != null)
        {
            newDummyBall.SetActive(false);
        }
    }

    void OnDestroy()
    {
        killAllObstacles();
    }
}
