using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Collections;
using System.Threading.Tasks;

public class SpinWheelController : MonoBehaviour
{
    [SerializeField] GameObject slotCardItem, itemScroll;
    [SerializeField] MonsterScriptable monsterScriptable;
    [SerializeField] List<SlotItemView> listSlotItem;
    [SerializeField] List<GameObject> listItemScroll;
    [SerializeField] RectTransform rectContentScroll;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float acceleration;
    private bool isStart = false;
    private bool isStop = false;
    float currentSpeed, currentDelay;
    float itemHeight;
    int currentIndex;
    float minDelay = 0.05f;
    float maxDelay = 1f;
    float delayTime = 0f;

    [SerializeField] int resultSlot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadDataItem();
    }

    void LoadDataItem()
    {
        itemHeight = (float)(itemScroll.GetComponent<RectTransform>()?.sizeDelta.y);
        currentDelay = maxDelay;
        if (listSlotItem.Count >= monsterScriptable.m_spr_monsnter.Count)
        {
            int indexMonsterData = 0;
            for (int i = 0; i <= listSlotItem.Count - 1; i++)
            {
                int index = i;

                var slotItem = listSlotItem[index];
                slotItem.SlotIndex = index;
                slotItem.gameObject.SetActive(true);
                slotItem.ImgMonster.sprite = monsterScriptable.m_spr_monsnter[indexMonsterData];
                slotItem.ImgDark.sprite = monsterScriptable.m_spr_monsnter[indexMonsterData];
                
                //TODO: Loading data monster scroll
                GameObject monsterOb = Instantiate(itemScroll, rectContentScroll);
                monsterOb.name = "Monster_" + i;
                monsterOb.transform.localPosition = new  Vector3(0, -(itemHeight * index), 0);
                
                var imgMonster = monsterOb.transform.Find("Icon")?.GetComponent<Image>();
                if (imgMonster != null)
                    imgMonster.sprite = monsterScriptable.m_spr_monsnter[indexMonsterData];
                listItemScroll.Add(monsterOb);
                
                indexMonsterData++;
                if (indexMonsterData > monsterScriptable.m_spr_monsnter.Count - 1)
                    indexMonsterData = 0;
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (delayTime > 0)
            delayTime -= Time.deltaTime;
        else
            delayTime = 0;

        if (Input.GetKeyDown(KeyCode.Space) && delayTime <= 0)
        {
            if(isStop) return;

            if (!isStart)
                StartSpin();
            else
            {
                if(!isStop)
                    StopSpin();
            }
            delayTime = 2f;
        }
        Scroll();
    }

    void StartSpin()
    {
        isStart = true;
        StartCoroutine(IESpinWheel());
        Debug.Log("Start");

    }

    void StopSpin()
    {
        isStart = false;
        isStop = true;
        var curIndex = currentIndex;

        resultSlot = Mathf.Abs(curIndex - (listSlotItem.Count - 1)) <= listSlotItem.Count / 2
                            ? curIndex + 3 
                            : 0;
        resultSlot = currentIndex -3 >= 0
                    ? (currentIndex - 3) 
                    : ((currentIndex - 3) + listSlotItem.Count);
        StartCoroutine(IEStopSpin());
        Debug.Log("Stop");
        
    }

    void Scroll()
    {
        float limitItem = itemHeight * rectContentScroll.childCount -1;
        if (rectContentScroll.anchoredPosition.y >= limitItem)
        {
            rectContentScroll.anchoredPosition -= Vector2.up * limitItem;
            return;
        }

        if (isStart)
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                maxSpeed,
                acceleration * Time.deltaTime);
                
            currentDelay = Mathf.Lerp(
                currentDelay,
                minDelay,
                maxDelay  * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(
                    currentSpeed,
                    0,
                    acceleration  * Time.deltaTime);

            currentDelay = Mathf.Lerp(
                            currentDelay,
                            maxDelay,
                            maxDelay * Time.deltaTime);

            if (isStop && currentSpeed <= acceleration * 2f)
            {
                SnapToCenter();
                return;
            }

        }
        rectContentScroll.anchoredPosition += Vector2.up * currentSpeed * Time.deltaTime;
    }

    void SnapToCenter()
    {
        var target = new Vector3(0, (resultSlot * itemHeight), 0);
        var posScroll = rectContentScroll.anchoredPosition;
        var indexItem = Mathf.RoundToInt(posScroll.y / itemHeight);
        var limitItem = itemHeight * rectContentScroll.childCount - 1;

        if (rectContentScroll.anchoredPosition.y >= limitItem)
        {
            rectContentScroll.anchoredPosition = Vector2.zero;
            return;
        }

        var speed = currentSpeed <= acceleration * 2f ? acceleration * 2f : currentSpeed;

        if (Mathf.Abs(posScroll.y - target.y) <= itemHeight * 0.5f)
        {
            posScroll.y = Mathf.MoveTowards(
                            posScroll.y,
                            target.y,
                            speed  * Time.deltaTime);            
        }
        else
        {
            posScroll.y = Mathf.MoveTowards(
                            posScroll.y,
                            limitItem,
                            speed  * Time.deltaTime);
        }
        rectContentScroll.anchoredPosition = posScroll;            

        if(Mathf.Abs(indexItem - resultSlot) == 0 && Mathf.Abs(posScroll.y - target.y) <= 0.1f && delayTime <= 0)
        {
            Result();
        }
    }

    private IEnumerator IESpinWheel()
    {
        while(isStart)
        {
            NextItem();
        
            yield return new WaitForSeconds(currentDelay);
        }
    }

    private IEnumerator IEStopSpin()
    {
        while (Mathf.Abs(resultSlot - currentIndex) != 0)
        {
            NextItem();
            yield return new WaitForSeconds(currentDelay * 0.75f);
        }
    }

    void NextItem()
    {
        var items = listSlotItem;
        items[currentIndex].SetDark(true);

        currentIndex++;

        if (currentIndex >= items.Count)
            currentIndex = 0;

        items[currentIndex].SetDark(false);
    }

    void Result()
    {
        var target = new Vector3(0, (resultSlot * itemHeight), 0);
        var posScroll = rectContentScroll.anchoredPosition;
        while (target.y != posScroll.y)
        {
            posScroll.y = Mathf.MoveTowards(
                            posScroll.y,
                            target.y,
                            acceleration  * Time.deltaTime);

            rectContentScroll.anchoredPosition = posScroll;
            Result();              
        }

        ResetData();
    }

    void ResetData()
    {
        isStart = isStop = false;
    }
}
