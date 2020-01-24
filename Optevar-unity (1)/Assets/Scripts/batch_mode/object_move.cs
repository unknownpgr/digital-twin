using UnityEngine;

public class object_move : MonoBehaviour
{
    object_button object_button;//mode_num받아옴

    public GameObject ori_person;
    public GameObject ori_sensor;
    public GameObject ori_exit;
    
    private GameObject chosen_object;
    private Vector3 mouse_pos;
    Vector3 pre_hit_pos = Vector3.zero;
    private void Awake()
    {
        object_button = GameObject.Find("all_objects").GetComponent<object_button>();//empty_object : 컴포넌트가 부착된 오브젝트
        
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnMouseDrag()
    {
        mouse_pos = Input.mousePosition;//드래그 시작할 때 마우스 위치 받아오기
        Ray cast_point = Camera.main.ScreenPointToRay(mouse_pos);
        RaycastHit hit;
        
        if (object_button.mode_num == 0 && Physics.Raycast(cast_point, out hit, Mathf.Infinity))
        {
            //person객체의 경우
            if (hit.collider.gameObject == ori_person) {
                chosen_object = ori_person;

            }//sensor
            else if (hit.collider.gameObject == ori_sensor)
            {
                chosen_object = ori_sensor;
            }//exit
            else if (hit.collider.gameObject == ori_exit)
            {
                chosen_object = ori_exit;
            }
            
            if (pre_hit_pos != Vector3.zero)
            {
                Vector3 tmp = hit.point - pre_hit_pos;
                tmp.y = 0;
                
                chosen_object.transform.position += tmp;
            }
            pre_hit_pos = hit.point;
           
            
        }
    }

    public void OnMouseUp()
    {
        pre_hit_pos = Vector3.zero;
    }
}
