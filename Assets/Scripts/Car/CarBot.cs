using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;  // Для использования NavMesh

public class CarBot : Car
{
    [SerializeField] float botTurnStrength = 5f;
    [SerializeField] CarProgressHandler progressHandler;
    [SerializeField] float baseSpeed = 1000f;  // базовая скорость
    LapHandler lapHandler;
    private NavMeshAgent navMeshAgent;  // Для управления навигацией

    public Vector3 Destination { get; set; }

    void Start()
    {
        // Получаем NavMeshAgent
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent не найден!");
            return;
        }

        // Отключаем автоматическое управление перемещением NavMeshAgent
        navMeshAgent.updatePosition = false;
        navMeshAgent.updateRotation = false;

        progressHandler.OnBotFinishedLap += HandleBotFinishedLap;
        progressHandler.OnBotCrossedCheckpoint += HandleBotCrossedCheckpoint;
        lapHandler = FindObjectOfType<LapHandler>();
        Destination = lapHandler.Checkpoints[0].GetRandomDestination();
        navMeshAgent.destination = Destination;  // Устанавливаем начальное направление
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
        navMeshAgent.destination = Destination;  // Обновляем цель
    }

    void HandleBotCrossedCheckpoint(CarBot bot, int checkpointIndex)
    {
        var checkpoints = lapHandler.Checkpoints;
        Destination = lapHandler.FinalCheckpoint == checkpointIndex ?
            lapHandler.gameObject.transform.position :
            checkpoints[checkpointIndex + 1].GetRandomDestination();

        navMeshAgent.destination = Destination;  // Обновляем цель
    }

    void LateUpdate()
    {
        BotMove();  // Движение бота
        base.LateUpdate();  // Обновляем Car класс
    }

    void BotMove()
    {
        // Устанавливаем скорость на основе сложности
        Speed = baseSpeed * Time.deltaTime;  // Движение с учетом базовой скорости и времени

        if (GroundChecker.OnGround)
        {
            State = CarState.OnGroundAndMovingForward;
            MoveForward();  // Перемещаем машину вперед
        }
        else
        {
            State = CarState.OffGround;
        }

        RotateBotTowardsDestination();  // Поворачиваем бота к цели
    }

    void MoveForward()
    {
        // Двигаем машину вперед с постоянной скоростью
        transform.position += transform.forward * Speed;
    }

    void RotateBotTowardsDestination()
    {
        // Используем NavMeshAgent для определения направления к цели
        Vector3 relativePos = navMeshAgent.steeringTarget - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(relativePos);
        float randomTurnStrength = Random.Range(botTurnStrength, botTurnStrength * 2);

        // Поворачиваем машину плавно в сторону цели
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * randomTurnStrength);
    }
}