package com.codehealth.scanners.java;

import java.util.*;
import java.io.*;
import org.json.simple.JSONArray;
import org.json.simple.JSONObject;

@SuppressWarnings("unchecked")
public class TodoScanner {
    public static void scan(Map<String, String> filesContent, String outputPath) {
        JSONArray filesArray = new JSONArray();

        for (Map.Entry<String, String> entry : filesContent.entrySet()) {
            String filePath = entry.getKey();
            String content = entry.getValue();

            JSONArray todos = new JSONArray();
            String[] lines = content.split("\n");

            for (int i = 0; i < lines.length; i++) {
                String line = lines[i];
                if (line.contains("TODO")) {
                    JSONObject todo = new JSONObject();
                    todo.put("line", i + 1);
                    todo.put("text", line.trim());

                    // Optional: Add a few lines of context
                    JSONArray context = new JSONArray();
                    for (int j = Math.max(0, i - 2); j <= Math.min(lines.length - 1, i + 2); j++) {
                        context.add(lines[j]);
                    }
                    todo.put("context", context);

                    todos.add(todo);
                }
            }

            if (!todos.isEmpty()) {
                JSONObject fileJson = new JSONObject();
                fileJson.put("file", filePath.replace("\\", "/"));  // Normalize path
                fileJson.put("todos", todos);
                filesArray.add(fileJson);
            }
        }

        try (FileWriter writer = new FileWriter(outputPath)) {
            writer.write(filesArray.toJSONString());
        } catch (IOException e) {
            e.printStackTrace();
        }
    }
}
