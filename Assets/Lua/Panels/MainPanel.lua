

function Awake()
    print("Awake:"..gameObject.name)

end

function OnEnable()
    print("OnEnable:"..transform.name)
end

function Start()
    print("Start")
end

function OnDisable()
    print("OnDisable")
end

function OnDestroy()
    print("OnDestroy")
end