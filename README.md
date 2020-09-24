# Dilute_Points
*Dilute points of lines. Input is xml file which contains points, output is the diluted xml file that only contains diluted points. Points satisfy the following condition will be removed: angle of the adjant two segments less than the specified angle(eg. 10°).<br>
对线中的点进行抽稀。输入为包含点的xml文件，输出为包含抽稀过后的点的xml文件。点将被删除，当满足下列条件时：相邻线段的夹角小于给定角度（如10°）。*
# Run
Dilute_Points\Dilute_Points\bin\Debug> Dilute_Points.exe
# Input xml snippet
```xml
    <Inflextion>
     <id>282</id>
     <name></name>
     <inflextionType>1</inflextionType>
     <dir>2</dir>
     <oX>564418.5</oX>
     <oY>572442.5</oY>
     <startX>564418.5</startX>
     <startY>572442.5</startY>
     <endX>564418.5</endX>
     <endY>572442.5</endY>
     <radius>0</radius>
    </Inflextion>
    <Inflextion>
     <id>283</id>
     <name></name>
     <inflextionType>1</inflextionType>
     <dir>1</dir>
     <oX>564429.5</oX>
     <oY>572449.3</oY>
     <startX>564429.5</startX>
     <startY>572449.3</startY>
     <endX>564429.5</endX>
     <endY>572449.3</endY>
     <radius>0</radius>
    </Inflextion>
    <Inflextion>
     <id>284</id>
     <name></name>
     <inflextionType>1</inflextionType>
     <dir>2</dir>
     <oX>564440.3</oX>
     <oY>572456.1</oY>
     <startX>564440.3</startX>
     <startY>572456.1</startY>
     <endX>564440.3</endX>
     <endY>572456.1</endY>
     <radius>0</radius>
    </Inflextion>
```

# Output xml snippet
```xml
    <Inflextion>
     <id>282</id>
     <name></name>
     <inflextionType>1</inflextionType>
     <dir>2</dir>
     <oX>564418.5</oX>
     <oY>572442.5</oY>
     <startX>564418.5</startX>
     <startY>572442.5</startY>
     <endX>564418.5</endX>
     <endY>572442.5</endY>
     <radius>0</radius>
    </Inflextion>
    <Inflextion>
     <id>284</id>
     <name></name>
     <inflextionType>1</inflextionType>
     <dir>2</dir>
     <oX>564440.3</oX>
     <oY>572456.1</oY>
     <startX>564440.3</startX>
     <startY>572456.1</startY>
     <endX>564440.3</endX>
     <endY>572456.1</endY>
     <radius>0</radius>
    </Inflextion>
```
