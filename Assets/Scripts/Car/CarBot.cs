using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;  // ��� ������������� NavMesh

public class CarBot : Car
{
    [SerializeField] float botTurnStrength = 5f;
    [SerializeField] CarProgressHandler progressHandler;
    [SerializeField] float baseSpeed = 1000f;  // ������� ��������
    LapHandler lapHandler;
    private NavMeshAgent navMeshAgent;  // ��� ���������� ����������

    public Vector3 Destination { get; set; }

    void Start()
    {
        // �������� NavMeshAgent
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent �� ������!");
            return;
        }

        // ��������� �������������� ���������� ������������ NavMeshAgent
        navMeshAgent.updatePosition = false;
        navMeshAgent.updateRotation = false;

        progressHandler.OnBotFinishedLap += HandleBotFinishedLap;
        progressHandler.OnBotCrossedCheckpoint += HandleBotCrossedCheckpoint;
        lapHandler = FindObjectOfType<LapHandler>();
        Destination = lapHandler.Checkpoints[0].GetRandomDestination();
        navMeshAgent.destination = Destination;  // ������������� ��������� �����������
    }

    void OnDestroy()
    {
        progressHandler.OnBotFinishedLap -= HandleBotFinishedLap;
        progressHandler.OnBotCrossedCheckpoint -= HandleBotCrossedCheckpoint;
    }

    void HandleBotFinishedLap(CarBot bot)
    {
        var checkpoints = lapHandler.Checkpoints;
        Destination = checkpoints[0].GetRandomDestination();
        navMeshAgent.destination = Destination;  // ��������� ����
    }

    void HandleBotCrossedCheckpoint(CarBot bot, int checkpointIndex)
    {
        var checkpoints = lapHandler.Checkpoints;
        Destination = lapHandler.FinalCheckpoint == checkpointIndex ?
            lapHandler.gameObject.transform.position :
            checkpoints[checkpointIndex + 1].GetRandomDestination();

        navMeshAgent.destination = Destination;  // ��������� ����
    }

    void LateUpdate()
    {
        BotMove();  // �������� ����
        base.LateUpdate();  // ��������� Car �����
    }

    void BotMove()
    {
        // ������������� �������� �� ������ ���������
        Speed = baseSpeed * Time.deltaTime;  // �������� � ������ ������� �������� � �������

        if (GroundChecker.OnGround)
        {
            State = CarState.OnGroundAndMovingForward;
            MoveForward();  // ���������� ������ ������
        }
        else
        {
            State = CarState.OffGround;
        }

        RotateBotTowardsDestination();  // ������������ ���� � ����
    }

    void MoveForward()
    {
        // ������� ������ ������ � ���������� ���������
        transform.position += transform.forward * Speed;
    }

    void RotateBotTowardsDestination()
    {
        // ���������� NavMeshAgent ��� ����������� ����������� � ����
        Vector3 relativePos = navMeshAgent.steeringTarget - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(relativePos);
        float randomTurnStrength = Random.Range(botTurnStrength, botTurnStrength * 2);

        // ������������ ������ ������ � ������� ����
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * randomTurnStrength);
    }
}