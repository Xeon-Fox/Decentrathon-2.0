using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarBot : Car
{
    [SerializeField] float botTurnStrength = 5f;
    [SerializeField] CarProgressHandler progressHandler;
    [SerializeField] float baseSpeed = 1000f;  // базовая скорость
    LapHandler lapHandler;
    DifficultyChooser difficultyChooser;

    public Vector3 Destination { get; set; }

    void Start()
    {
        progressHandler.OnBotFinishedLap += HandleBotFinishedLap;
        progressHandler.OnBotCrossedCheckpoint += HandleBotCrossedCheckpoint;
        lapHandler = FindObjectOfType<LapHandler>();
        difficultyChooser = FindObjectOfType<DifficultyChooser>(); // Получаем ссылку на DifficultyChooser

        if (difficultyChooser == null)
        {
            Debug.LogError("DifficultyChooser не найден!");
        }
        else
        {
            // Установим сложность сразу при старте
            AdjustSpeedAndTurnStrength();
        }

        Destination = lapHandler.Checkpoints[0].GetRandomDestination();
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
    }

    void HandleBotCrossedCheckpoint(CarBot bot, int checkpointIndex)
    {
        var checkpoints = lapHandler.Checkpoints;
        Destination = lapHandler.FinalCheckpoint == checkpointIndex ?
            lapHandler.gameObject.transform.position :
            checkpoints[checkpointIndex + 1].GetRandomDestination();
    }

    void LateUpdate()
    {
        BotMove();
        base.LateUpdate();
    }

    void BotMove()
    {
        AdjustSpeedAndTurnStrength(); // Настройка скорости и силы поворота
        Speed = ForwardAccel * baseSpeed;

        // Check if the car is on the "grass" layer
        if (gameObject.layer == LayerMask.NameToLayer("Grass"))
        {
            // Use raycasting to find the nearest point on the "ground" layer
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
            {
                // Adjust the car's position to the hit point
                transform.position = hit.point + Vector3.up * 0.5f; // Adjust height slightly to avoid sinking
            }
        }

        if (GroundChecker.OnGround) State = CarState.OnGroundAndMovingForward;
        else State = CarState.OffGround;
        RotateBotTowardsDestination();
    }

    void AdjustSpeedAndTurnStrength()
    {
        // Проверяем наличие difficultyChooser
        if (difficultyChooser != null)
        {
            switch (difficultyChooser.selectedDifficulty)
            {
                case 1: // Легкая сложность
                    baseSpeed = 400f;  // Уменьшаем скорость
                    botTurnStrength = 3f;  // Уменьшаем силу поворота
                    break;
                case 2: // Средняя сложность
                    baseSpeed = 800f;  // Базовая скорость
                    botTurnStrength = 5f;  // Базовая сила поворота
                    break;
                case 3: // Высокая сложность
                    baseSpeed = 1200f;  // Увеличиваем скорость
                    botTurnStrength = 7f;  // Увеличиваем силу поворота
                    break;
                default:
                    Debug.LogWarning("Неизвестный уровень сложности: " + difficultyChooser.selectedDifficulty);
                    break;
            }
        }
        else
        {
            Debug.LogWarning("DifficultyChooser не инициализирован.");
        }
    }

    void RotateBotTowardsDestination()
    {
        Vector3 relativePos = Destination - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(relativePos);
        float randomTurnStrength = Random.Range(botTurnStrength, botTurnStrength * 2);

        Vector3 oldRotation = transform.rotation.eulerAngles;

        if (GroundChecker.OnGround)
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * randomTurnStrength);

        if ((oldRotation - transform.rotation.eulerAngles).y > 0.05f)
            TurnInput = Mathf.Lerp(TurnInput, -1, Time.deltaTime * 10);
        else if ((oldRotation - transform.rotation.eulerAngles).y < -0.05f)
            TurnInput = Mathf.Lerp(TurnInput, 1, Time.deltaTime * 10);
        else TurnInput = Mathf.Lerp(TurnInput, 0, Time.deltaTime * 10);
    }
}