using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Prediction3D : Singleton<Prediction3D>
{
    public DiceMachine diceMachine;
    public GameObject obstacles;
    public int maxIterations;
    [SerializeField] private int diceCount = 0;
    [SerializeField] private List<Transform> firePointList;

    Scene currentScene;
    Scene predictionScene;

    PhysicsScene currentPhysicsScene;
    PhysicsScene predictionPhysicsScene;

    List<GameObject> dummyObstacles = new List<GameObject>();

    [SerializeField] private LineRenderer lineRenderer;
    List<LineRenderer> lineRenderers = new List<LineRenderer>();
    [HideInInspector] public List<GameObject> newDices;

    [SerializeField] private float pushPower;
    [SerializeField] private float rotationPower;

    private List<Vector3> currentPositions = new List<Vector3>();
    private List<Quaternion> currentRotations = new List<Quaternion>();

    [HideInInspector] public List<List<(Vector3, Vector3)>> linePositionsList = new List<List<(Vector3, Vector3)>>();

    void Start()
    {
        Init();

        for(var f = 0; f < firePointList.Count; f++)
        {
            currentPositions.Add(firePointList[f].position);
            currentRotations.Add(firePointList[f].rotation);
        }


        if (currentPhysicsScene.IsValid() && diceMachine.isTest)
        {
            copyAllObstacles();
            predict();
        }
    }

    public void ClearLineRenderers()
    {
        for(var i = 0; i < lineRenderers.Count; i++)
        {
            lineRenderers[i].positionCount = 0;
            lineRenderers[i].SetPositions(new Vector3[0]);
            lineRenderers[i].positionCount = maxIterations;
        }
    }

    public Vector2 calculateForce()
    {
        return new Vector2(0f, 1f * pushPower);
    }

    void shoot()
    {
        //GameObject ball = Instantiate(dicePrefab, firePoint.transform.position, Quaternion.identity);
        //ball.GetComponent<Rigidbody2D>().AddForce(calculateForce(), ForceMode2D.Impulse);
    }

    public void MoveFirePoint(int targetShooterIndex, float x, float y)
    {
        firePointList[targetShooterIndex].position = new Vector3(x, y, 0f);

        predict();
    }

    public void Init()
    {
        currentScene = SceneManager.GetActiveScene();
        currentPhysicsScene = currentScene.GetPhysicsScene();

        CreateSceneParameters parameters = new CreateSceneParameters(LocalPhysicsMode.Physics3D);
        predictionScene = SceneManager.CreateScene("Prediction3D", parameters);
        predictionPhysicsScene = predictionScene.GetPhysicsScene();
    }
    void FixedUpdate()
    {
        if (currentPhysicsScene.IsValid() && diceMachine.isTest)
        {
            var changePosOrRot = false;

            for(var p = 0; p < currentPositions.Count; p++)
            {
                if (currentPositions[p] != firePointList[p].position)
                {
                    currentPositions[p] = firePointList[p].position;
                    changePosOrRot = true;
                    break;
                }
            }
            for (var r = 0; r < currentPositions.Count; r++)
            {
                if (currentRotations[r] != firePointList[r].rotation)
                {
                    currentRotations[r] = firePointList[r].rotation;
                    changePosOrRot = true;
                    break;
                }
            }

            if (changePosOrRot == true)
            {
                predict();
            }

            currentPhysicsScene.Simulate(Time.fixedDeltaTime);
        }
    }

    public void copyAllObstacles()
    {
        if (newDices.Count < 1)
        {
            for (var i = 0; i < diceCount; i++)
            {
                var dice = Instantiate(diceMachine.dicePrefab);
                newDices.Add(dice);
                //dice.gameObject.transform.GetChild(0).gameObject.SetActive(false);
                SceneManager.MoveGameObjectToScene(dice, predictionScene);

                var lineRendererObj = Instantiate(lineRenderer, transform);
                lineRenderers.Add(lineRendererObj.GetComponent<LineRenderer>());
                lineRendererObj.gameObject.SetActive(true);
                linePositionsList.Add(new List<(Vector3, Vector3)>());
            }
        }
        foreach (Transform t in obstacles.transform)
        {
            if (t.gameObject.GetComponent<Collider>() != null)
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
    }

    void killAllObstacles()
    {
        foreach (var o in dummyObstacles)
        {
            Destroy(o);
        }
        dummyObstacles.Clear();
    }

    public void predict()
    {
        if (currentPhysicsScene.IsValid() && predictionPhysicsScene.IsValid())
        {
            for(var i = 0; i < newDices.Count; i++)
            {
                linePositionsList[i].Clear();
                newDices[i].SetActive(true);
                newDices[i].transform.position = firePointList[i].position;
                newDices[i].transform.localEulerAngles = firePointList[i].localEulerAngles;
                var newDiceRigid = newDices[i].GetComponent<Rigidbody>();
                newDiceRigid.velocity = new Vector3(0f,0f,0f);
                newDiceRigid.AddForce(firePointList[i].forward * pushPower, ForceMode.Impulse);
                newDiceRigid.AddTorque(new Vector3(rotationPower, rotationPower, rotationPower), ForceMode.Impulse);
            }

            ClearLineRenderers();

            for (int i = 0; i < maxIterations; i++)
            {
                predictionPhysicsScene.Simulate(0.03f);
                for(var l = 0; l < newDices.Count; l++)
                {
                    lineRenderers[l].SetPosition(i, newDices[l].transform.position);
                    var posRot = (newDices[l].transform.position, newDices[l].transform.eulerAngles);
                    linePositionsList[l].Add(posRot);
                }
            }

            ReturnDummyDice();
        }
    }

    public void ReturnDummyDice()
    {
        for(var i = 0; i < newDices.Count; i++)
        {
            if (newDices[i] != null)
            {
                newDices[i].SetActive(false);
            }
        }
    }

    void OnDestroy()
    {
        killAllObstacles();
    }
}
