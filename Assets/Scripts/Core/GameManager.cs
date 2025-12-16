using UnityEngine;
using DPBomberman.Patterns.State;

public class GameManager : MonoBehaviour
{
    [Header("Scene References")]
    public MapGenerator mapGenerator;
    public MapLogicAdapter mapLogicAdapter;

    private GameStateMachine stateMachine;

    private void Awake()
    {
        stateMachine = new GameStateMachine();
    }

    private void Start()
    {
        
        stateMachine.ChangeState(new MainMenuState(this, stateMachine));
    }

    private void Update()
    {
        stateMachine.Tick(Time.deltaTime);
    }
}