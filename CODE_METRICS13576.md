{
  "openapi": "3.0.1",
  "info": {
    "title": "Swagger Example (Simple)",
    "version": "1.0.0"
  },
  "servers": [
    {
      "url": "https://localhost:5001"
    }
  ],
  "paths": { },
  "components": {
    "schemas": {
      "CourseApi.Models.Article": {
        "type": "object",
        "allOf": [
          {
            "$ref": "#/components/schemas/CourseApi.Models.CreateEditModel"
          }
        ],
        "properties": {
          "Id": {
            "type": "integer",
            "format": "int32"
          },
          "SurveyId": {
            "type": "integer",
            "format": "int32"
          },
          "ArticleValue": {
            "type": "string",
            "nullable": true
          },
          "ArticleTitle": {
            "type": "string",
            "nullable": true
          },
          "Survey": {
            "$ref": "#/components/schemas/CourseApi.Models.Survey"
          }
        }
      },
      "CourseApi.Models.Survey": {
        "type": "object",
        "allOf": [
          {
            "$ref": "#/components/schemas/CourseApi.Models.CreateEditModel"
          }
        ],
        "properties": {
          "Id": {
            "type": "integer",
            "format": "int32"
          },
          "Name": {
            "type": "string",
            "nullable": true
          },
          "Questions": {
            "$ref": "#/components/schemas/System.Collections.Generic.ICollection<T>CourseApi.Models.Question"
          },
          "Article": {
            "$ref": "#/components/schemas/CourseApi.Models.Article"
          }
        }
      },
      "CourseApi.Models.Question": {
        "required": [
          "QuestionValue"
        ],
        "type": "object",
        "allOf": [
          {
            "$ref": "#/components/schemas/CourseApi.Models.CreateEditModel"
          }
        ],
        "properties": {
          "Id": {
            "type": "integer",
            "format": "int32"
          },
          "SurveyId": {
            "type": "integer",
            "format": "int32"
          },
          "QuestionTypeId": {
            "type": "integer",
            "format": "int32"
          },
          "QuestionValue": {
            "type": "string",
            "nullable": true
          },
          "SliderDetail": {
            "$ref": "#/components/schemas/CourseApi.Models.SliderDetail"
          },
          "Survey": {
            "$ref": "#/components/schemas/CourseApi.Models.Survey"
          },
          "QuestionType": {
            "$ref": "#/components/schemas/CourseApi.Models.QuestionType"
          },
          "MatrixColumns": {
            "$ref": "#/components/schemas/System.Collections.Generic.ICollection<T>CourseApi.Models.MatrixColumn"
          },
          "Options": {
            "$ref": "#/components/schemas/System.Collections.Generic.ICollection<T>CourseApi.Models.Option"
          }
        }
      },
      "System.Collections.Generic.ICollection<T>CourseApi.Models.SliderMark": {
        "type": "object"
      },
      "System.Collections.Generic.ICollection<T>CourseApi.Models.MatrixColumn": {
        "type": "object"
      },
      "System.Collections.Generic.ICollection<T>CourseApi.Models.Option": {
        "type": "object"
      },
      "Microsoft.Extensions.DependencyInjection.IServiceCollection": {
        "type": "object"
      },
      "Microsoft.AspNetCore.Builder.IApplicationBuilder": {
        "type": "object"
      },
      "Microsoft.AspNetCore.Hosting.IWebHostEnvironment": {
        "type": "object"
      },
      "CourseApi.Models.CreateEditModel": {
        "type": "object",
        "properties": {
          "CreatedOnDate": {
            "type": "string",
            "format": "date-time"
          },
          "UpdatedOnDate": {
            "type": "string",
            "format": "date-time"
          }
        }
      },
      "System.Collections.Generic.ICollection<T>CourseApi.Models.Question": {
        "type": "object"
      },
      "CourseApi.Models.SliderDetail": {
        "type": "object",
        "allOf": [
          {
            "$ref": "#/components/schemas/CourseApi.Models.CreateEditModel"
          }
        ],
        "properties": {
          "Id": {
            "type": "integer",
            "format": "int32"
          },
          "QuestionId": {
            "type": "integer",
            "format": "int32"
          },
          "Max": {
            "type": "string",
            "nullable": true
          },
          "Step": {
            "type": "string",
            "nullable": true
          },
          "Min": {
            "type": "string",
            "nullable": true
          },
          "InitialValue": {
            "type": "string",
            "nullable": true
          },
          "Question": {
            "$ref": "#/components/schemas/CourseApi.Models.Question"
          },
          "SliderMarks": {
            "$ref": "#/components/schemas/System.Collections.Generic.ICollection<T>CourseApi.Models.SliderMark"
          }
        }
      },
      "CourseApi.Models.QuestionType": {
        "type": "object",
        "properties": {
          "Id": {
            "type": "integer",
            "format": "int32"
          },
          "Key": {
            "type": "integer",
            "format": "int32"
          },
          "Value": {
            "type": "string",
            "nullable": true
          }
        }
      }
    }
  }
}