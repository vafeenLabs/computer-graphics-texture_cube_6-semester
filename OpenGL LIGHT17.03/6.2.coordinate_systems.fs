#version 330 core
out vec4 FragColor;

// Структура материала объекта
struct Material {
    sampler2D diffuse;   // Диффузная текстура
    vec3 specular;      // Цвет зеркального отражения
    float shininess;    // Степень блеска (размер блика)
}; 

// Структура прожекторного света (SpotLight)
struct SpotLight {
    vec3 position;      // Позиция источника света
    vec3 direction;     // Направление луча прожектора
    float cutOff;       // косинус угла внутреннего конуса (основная яркая часть луча)
    float outerCutOff;  // косинус угла внутреннего конуса (основная яркая часть луча)
  
    vec3 ambient;      // Фоновая компонента света
    vec3 diffuse;      // Диффузная компонента света // где свет рассеивается 
    vec3 specular;     // Зеркальная компонента света
    
    // Коэффициенты затухания света с расстоянием
    float constant;    // Постоянная составляющая
    float linear;      // Линейная составляющая
    float quadratic;   // Квадратичная составляющая
};

//attenuation= 1/(constant+linear*d+quadratic*d^2)


// Входные данные из вершинного шейдера
in vec2 TexCoords;     // Текстурные координаты
in vec3 Normal;        // Нормаль фрагмента
in vec3 FragPos;       // Позиция фрагмента в мировых координатах

// Uniform-переменные
uniform vec3 viewPos;  // Позиция камеры
uniform Material material;  // Материал объекта
uniform SpotLight spotLight;  // Прожекторный источник света

void main()
{
    // 1. Фоновая составляющая освещения (ambient)
    // Умножаем фоновый свет на цвет из текстуры
    vec3 ambient = spotLight.ambient * texture(material.diffuse, TexCoords).rgb;
    
    // 2. Диффузная составляющая освещения
    // Нормализуем нормаль (может быть интерполирована)
    vec3 norm = normalize(Normal);
    // Направление от фрагмента к источнику света
    vec3 lightDir = normalize(spotLight.position - FragPos);
    // Вычисляем угол между нормалью и направлением света (диффузная составляющая)
    float diff = max(dot(norm, lightDir), 0.0);
    // Умножаем диффузный свет на цвет из текстуры и коэффициент диффузного освещения
    vec3 diffuse = spotLight.diffuse * diff * texture(material.diffuse, TexCoords).rgb;  
    
    // 3. Зеркальная составляющая освещения (блики)
    // Направление от фрагмента к камере
    vec3 viewDir = normalize(viewPos - FragPos);
    // Направление отраженного света (отражаем вектор света относительно нормали)
    vec3 reflectDir = reflect(-lightDir, norm);  
    // Вычисляем зеркальную составляющую (чем больше угол между отраженным светом и направлением к камере, тем меньше блик)
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    // Умножаем зеркальный свет на цвет материала и коэффициент зеркального освещения
    vec3 specular = spotLight.specular * spec * material.specular;  
    
    // 4. Эффект прожектора (мягкие границы)
    // Вычисляем косинус угла между направлением света и направлением прожектора
    float theta = dot(lightDir, normalize(-spotLight.direction)); 
    // Разница между внутренним и внешним углом конуса
    float epsilon = spotLight.cutOff - spotLight.outerCutOff;
    // Интерполяция интенсивности между внутренним и внешним конусом
    // Дает плавное затухание света на границе конуса
    float intensity = clamp((theta - spotLight.outerCutOff) / epsilon, 0.0, 1.0);
    // Применяем интенсивность к диффузной и зеркальной составляющим
    diffuse *= intensity;
    specular *= intensity;
    
    // 5. Затухание света с расстоянием (attenuation)
    // Вычисляем расстояние от источника света до фрагмента
    float distance = length(spotLight.position - FragPos);
    // Формула затухания: 1.0 / (constant + linear*distance + quadratic*(distance^2))
    float attenuation = 1.0 / (spotLight.constant + spotLight.linear * distance + 
                             spotLight.quadratic * (distance * distance));    
    // Применяем затухание ко всем компонентам света
    ambient *= attenuation;
    diffuse *= attenuation;
    specular *= attenuation;
    
    // Суммируем все компоненты освещения
    vec3 result = ambient + diffuse + specular;
    // Устанавливаем итоговый цвет фрагмента (альфа = 1.0 - полностью непрозрачный)
    FragColor = vec4(result, 1.0);
}