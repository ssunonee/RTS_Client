using UnityEngine;

public class Unit : MonoBehaviour {

    public int id;
    private Animator anim;
    public bool selected;

    public Material selected_mat;
    public Material unselected_mat;
    private MeshRenderer rend;

    private bool moving;
    public Vector3 last_pos;
    public Vector3 next_pos;
    private float last_progress;
    private float next_progress;

    private float interpolation_progress;

    private void Awake()
    {
        rend = GetComponent<MeshRenderer>();
        SetSelected(false);
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (moving == false)
            return;

        interpolation_progress += Time.deltaTime * 5;
        float interpolated = Mathf.Lerp(last_progress, next_progress, interpolation_progress);
        transform.position = Vector3.Lerp(next_pos, last_pos, interpolated);
    }

    public void UpdateState(UnitDTO dto)
    {
        if (dto == null || anim == null)
            return;

        if (dto.Moving == true)
        {
            moving = true;
            if (anim.GetBool("moving") == false)
            {
                anim.SetBool("moving", true);
            }
        }
        else
        {
            moving = false;
            if (anim.GetBool("moving") == true)
                anim.SetBool("moving", false);
        }

        if (dto.MoveProgress > 0)
        {
            last_progress = next_progress;
            next_progress = dto.MoveProgress;
            interpolation_progress = 0f;

            last_pos = next_pos;
            next_pos = new Vector3(dto.NextPos.X, transform.position.y, dto.NextPos.Y);

            //float x = Mathf.Lerp(dto.ParentPos.X, dto.NextPos.X, dto.MoveProgress);
            //float z = Mathf.Lerp(dto.ParentPos.Y, dto.NextPos.Y, dto.MoveProgress);
            //transform.position = new Vector3(x, transform.position.y, z);
        }
        else
        {
            //transform.position = last_pos;
            next_pos = last_pos;
            //transform.position = new Vector3(dto.ParentPos.X, transform.position.y, dto.ParentPos.Y);
        }
    }

    public void SetSelected(bool state)
    {
        if (state)
        {
            rend.material = selected_mat;
            selected = true;
        }
        else
        {
            rend.material = unselected_mat;
            selected = false;
        }
    }
}
