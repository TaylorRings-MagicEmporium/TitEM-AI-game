using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CashManager : MonoBehaviour
{
    struct MoneyNameValue
    {
        public MoneyNameValue(string dest, int val)
        {
            this.dest = dest;
            this.val = val;
        }

        public string dest;
        public int val;
    }

    Queue<MoneyNameValue> TotalRecieved = new Queue<MoneyNameValue>();

    public int TotalMoney = 0;
    public int MoneyEarnedFloor = 0;

    public Text MoneyDescription;
    public Text MoneyAmount;
    public Text VisMoneyFromFloor;
    public Text VisTotalMoney;


    public Text GameOverTotal;
    bool finish = false;
    Coroutine ListValuesRunner;

    public void AddMoney(string dest, int val)
    {
        TotalRecieved.Enqueue(new MoneyNameValue(dest, val));
        MoneyEarnedFloor += val;
    }

    public void DisplayMoneySummary()
    {
        ListValuesRunner = StartCoroutine(ListValues());
    }

    public void FinishSummary()
    {
        if (ListValuesRunner != null)
        {
            StopCoroutine(ListValuesRunner);
        }
        if (!finish)
        {
            TotalMoney += MoneyEarnedFloor;
            finish = true;
        }
        Reset_Level();
    }

    public void Reset_Level()
    {
        MoneyEarnedFloor = 0;
        TotalRecieved.Clear();
        MoneyAmount.text = "";
        MoneyDescription.text = "";
        VisMoneyFromFloor.text = "";
        VisTotalMoney.text = "";
        finish = false;
    }

    public void Reset_Game()
    {
        TotalMoney = 0;
        Reset_Level();
    }

    private void Start()
    {
        Reset_Game();
    }

    public void DisplayFinalValue()
    {
        GameOverTotal.text = "Total Earned: \n$" + TotalMoney;
    }

    IEnumerator ListValues()
    {
        int b = TotalRecieved.Count;
        for(int i = 0; i < b; i++)
        {
            MoneyNameValue curr = TotalRecieved.Dequeue();
            //Debug.Log(curr.dest);
            MoneyDescription.text += curr.dest + "\n";
            MoneyAmount.text += "$" + curr.val + "\n";
            yield return new WaitForSeconds(0.5f);
        }


        if (!finish)
        {
            TotalMoney += MoneyEarnedFloor;
            finish = true;
        }

        VisMoneyFromFloor.text = "Money Earned: + $" + MoneyEarnedFloor;
        VisTotalMoney.text = "money in total: $" + TotalMoney;
    }
}
