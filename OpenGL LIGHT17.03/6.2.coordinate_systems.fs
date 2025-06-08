#version 330 core
out vec4 FragColor;

// ��������� ��������� �������
struct Material {
    sampler2D diffuse;   // ��������� ��������
    vec3 specular;      // ���� ����������� ���������
    float shininess;    // ������� ������ (������ �����)
}; 

// ��������� ������������� ����� (SpotLight)
struct SpotLight {
    vec3 position;      // ������� ��������� �����
    vec3 direction;     // ����������� ���� ����������
    float cutOff;       // ������� ���� ����������� ������ (�������� ����� ����� ����)
    float outerCutOff;  // ������� ���� ����������� ������ (�������� ����� ����� ����)
  
    vec3 ambient;      // ������� ���������� �����
    vec3 diffuse;      // ��������� ���������� ����� // ��� ���� ������������ 
    vec3 specular;     // ���������� ���������� �����
    
    // ������������ ��������� ����� � �����������
    float constant;    // ���������� ������������
    float linear;      // �������� ������������
    float quadratic;   // ������������ ������������
};

//attenuation= 1/(constant+linear*d+quadratic*d^2)


// ������� ������ �� ���������� �������
in vec2 TexCoords;     // ���������� ����������
in vec3 Normal;        // ������� ���������
in vec3 FragPos;       // ������� ��������� � ������� �����������

// Uniform-����������
uniform vec3 viewPos;  // ������� ������
uniform Material material;  // �������� �������
uniform SpotLight spotLight;  // ������������ �������� �����

void main()
{
    // 1. ������� ������������ ��������� (ambient)
    // �������� ������� ���� �� ���� �� ��������
    vec3 ambient = spotLight.ambient * texture(material.diffuse, TexCoords).rgb;
    
    // 2. ��������� ������������ ���������
    // ����������� ������� (����� ���� ���������������)
    vec3 norm = normalize(Normal);
    // ����������� �� ��������� � ��������� �����
    vec3 lightDir = normalize(spotLight.position - FragPos);
    // ��������� ���� ����� �������� � ������������ ����� (��������� ������������)
    float diff = max(dot(norm, lightDir), 0.0);
    // �������� ��������� ���� �� ���� �� �������� � ����������� ���������� ���������
    vec3 diffuse = spotLight.diffuse * diff * texture(material.diffuse, TexCoords).rgb;  
    
    // 3. ���������� ������������ ��������� (�����)
    // ����������� �� ��������� � ������
    vec3 viewDir = normalize(viewPos - FragPos);
    // ����������� ����������� ����� (�������� ������ ����� ������������ �������)
    vec3 reflectDir = reflect(-lightDir, norm);  
    // ��������� ���������� ������������ (��� ������ ���� ����� ���������� ������ � ������������ � ������, ��� ������ ����)
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
    // �������� ���������� ���� �� ���� ��������� � ����������� ����������� ���������
    vec3 specular = spotLight.specular * spec * material.specular;  
    
    // 4. ������ ���������� (������ �������)
    // ��������� ������� ���� ����� ������������ ����� � ������������ ����������
    float theta = dot(lightDir, normalize(-spotLight.direction)); 
    // ������� ����� ���������� � ������� ����� ������
    float epsilon = spotLight.cutOff - spotLight.outerCutOff;
    // ������������ ������������� ����� ���������� � ������� �������
    // ���� ������� ��������� ����� �� ������� ������
    float intensity = clamp((theta - spotLight.outerCutOff) / epsilon, 0.0, 1.0);
    // ��������� ������������� � ��������� � ���������� ������������
    diffuse *= intensity;
    specular *= intensity;
    
    // 5. ��������� ����� � ����������� (attenuation)
    // ��������� ���������� �� ��������� ����� �� ���������
    float distance = length(spotLight.position - FragPos);
    // ������� ���������: 1.0 / (constant + linear*distance + quadratic*(distance^2))
    float attenuation = 1.0 / (spotLight.constant + spotLight.linear * distance + 
                             spotLight.quadratic * (distance * distance));    
    // ��������� ��������� �� ���� ����������� �����
    ambient *= attenuation;
    diffuse *= attenuation;
    specular *= attenuation;
    
    // ��������� ��� ���������� ���������
    vec3 result = ambient + diffuse + specular;
    // ������������� �������� ���� ��������� (����� = 1.0 - ��������� ������������)
    FragColor = vec4(result, 1.0);
}