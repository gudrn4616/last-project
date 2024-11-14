using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;

public class Controller : FSMController<ControlState, ControlFSM, BaseDataSO>
{

    private void Start()
    {
        fsm = new ControlFSM(CreateState<ControlDayState>());
        Init(null);
    }

    public override void Init(BaseDataSO data)
    {
        StartCoroutine(Click());
    }

    public IEnumerator Click()
    {
        Debug.Log("Click");
        while (true)
        {
            yield return new WaitUntil(() => Input.GetMouseButtonUp(0));
            var mousePosition = Input.mousePosition;
            Ray ray = GameManager.instance.mainCamera.ScreenPointToRay(mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 20, Color.red, 5f);
            var hit = Physics2D.Raycast(ray.origin, ray.direction);
            if(hit)
            {
                currentState.OnClickScreen(hit);
            }
            yield return new WaitUntil(() => !Input.GetMouseButtonUp(0));
        }
    }

    private void Update()
    {
        if (UIGame.instance != null && UIGame.instance.stick.OnHandleChanged == null)
        {
            var dir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
            GameManager.instance.userCharacter?.MoveCharacter(dir);
        }
        fsm.UpdateState();
    }
}
