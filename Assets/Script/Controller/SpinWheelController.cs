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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(isStop) return;

            if (!isStart)
                StartSpin();
            else
            {
                if(!isStop)
                    StopSpin();
            }
        }
        Scroll();
    }

    void StartSpin()
    {
        isStart = true;
        Debug.Log("isStart:" + isStart);
        StartCoroutine(IESpinWheel());
    }

    void StopSpin()
    {
        isStart = false;

        Debug.Log("isStop :" + isStop);
        
    }

    void Result()
    {
        ResetData();
    }

    void Scroll()
    {
        float limitItem = itemHeight * rectContentScroll.childCount -1;
        if (rectContentScroll.anchoredPosition.y >= limitItem)
        {
            rectContentScroll.anchoredPosition -= Vector2.up * limitItem;
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
                    acceleration * Time.deltaTime);

            currentDelay = Mathf.MoveTowards(
                            currentDelay,
                            maxDelay,
                            maxDelay * Time.deltaTime);

            if (currentSpeed <= acceleration && isStop)
            {
                isStop = true;
                resultSlot = UnityEngine.Random.Range(currentIndex , listSlotItem.Count -1);
                currentSpeed  = acceleration;
                StartCoroutine(IEStopSpin());
                SnapToCenter(resultSlot);
                return;
            }

        }
        rectContentScroll.anchoredPosition += Vector2.up * currentSpeed * Time.deltaTime;
    }

    void SnapToCenter(int resultIndex)
    {
        var target = new Vector3(0, (resultIndex * itemHeight), 0);
        var posScroll = rectContentScroll.anchoredPosition;
        Debug.Log($"Result : {resultIndex}, currenIndex: {currentIndex}, Result lon hon: {resultIndex >= currentIndex}");
        
        if (resultIndex <= currentIndex)
        {
            float limitItem = itemHeight * rectContentScroll.childCount -2;
            if (rectContentScroll.anchoredPosition.y >= limitItem)
            {
                rectContentScroll.anchoredPosition -= Vector2.up * limitItem;
                return;
            }
            else
            {
                //rectContentScroll.anchoredPosition = Vector3.MoveTowards(rectContentScroll.anchoredPosition, target, acceleration * Time.deltaTime); 
                posScroll.y = Mathf.MoveTowards(
                        posScroll.y,
                        target.y,
                        acceleration * Time.deltaTime);
            }
        }
        else
        {                
            //rectContentScroll.anchoredPosition = Vector3.MoveTowards(rectContentScroll.anchoredPosition, target, acceleration * Time.deltaTime);
            posScroll.y = Mathf.MoveTowards(
                        posScroll.y,
                        target.y,
                        acceleration * Time.deltaTime);
        }
        
        rectContentScroll.anchoredPosition = posScroll;
        if(Mathf.Abs(posScroll.y - target.y) < 0.1f)
        {
            ResetData();
        }
        //rectContentScroll.anchoredPosition = Vector3.MoveTowards(rectContentScroll.anchoredPosition, target, 50f * Time.deltaTime);

        //ResetData();
    }

    private IEnumerator IESpinWheel()
    {
        while(isStart && !isStop)
        {
            NextItem();
        
            yield return new WaitForSeconds(currentDelay);
        }
    }

    private IEnumerator IEStopSpin()
    {
        while (resultSlot != currentIndex)
        {
            Debug.Log("IEStopSpin");
            NextItem();
            yield return new WaitForSeconds(currentDelay / 2f);
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

    void ResetData()
    {
        isStart = isStop = false;
        currentSpeed = 0;
    }
}
