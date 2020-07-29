using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SMSSender : MonoBehaviour
{
    // InputField about phone number
    public InputField inputPhoneNum;
    // Variable to save input phone number
    private string phoneNumber;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
 
    public void EditPhoneNumber()
    {
        // Set phone number to input value
        phoneNumber = inputPhoneNum.text;

        // If input value is invalid, pop up message
        if (phoneNumber == null || phoneNumber.Length < 1)
        {
            Popup.Show("유효한 전화번호를 입력하지 않았습니다.");
            return;
        }

        Popup.Show(phoneNumber +"로 전화번호가 저장되었습니다.");
    }
}
