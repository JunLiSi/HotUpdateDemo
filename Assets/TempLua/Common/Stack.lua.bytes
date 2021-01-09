--[[注意：只支持默认数字连续键值！！！]]
Stack = {}
local count = 0
function Stack:New(o)
    local x = o or {}
    x.stack_list ={}
    x.count = 0
    setmetatable(x, Stack)
    Stack.__index = Stack
    return x
end

function Stack:Push(element)

    -- for i, v in ipairs( self.stack_list ) do

    --     if v == element then
    --         table.remove(self.stack_list, i)
    --         break
    --     end
    -- end

    self.count = self.count+1
    --print(element)
    self.stack_list[self.count] = element
end

function Stack:Pop()
    
    if self.count < 1 then
        logError('stack_list count is 0!!')
        return
    end
    local pope = table.remove(self.stack_list, self.count)
    self.count = self.count-1
    return pope
end

function Stack:Count()
    return self.count
end

function Stack:GetTop()
     local top;
    if self.count > 0 then
        top = self.stack_list[self.count]
    end
    --print(top)
    return top
end

function Stack:Contain(element)
    for i=1, self.count do
        if element == self.stack_list[i] then
            return true
        end
    end
    return false
end

function Stack:GetList()
    return self.stack_list
end

function Stack:Remove(element)
    for i, v in ipairs( self.stack_list ) do
        if v == element then
            --print( "message...........................4" )
            self.count  =self.count - 1
           return table.remove(self.stack_list, i)
        end
    end
end

function Stack:Clear()
    self.stack_list = nil
    self.stack_list = {}
    self.count = 0
end