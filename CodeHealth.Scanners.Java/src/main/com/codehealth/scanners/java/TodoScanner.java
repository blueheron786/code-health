package com.codehealth.scanners.java;

import java.util.*;
import java.io.*;
import org.json.simple.JSONArray;
import org.json.simple.JSONObject;

public class TodoScanner {
    public static void run() {
        JSONArray todos = new JSONArray();

        JSONObject todo = new JSONObject();
        todo.put("File", "Example.java");
        todo.put("Line", 10);
        todo.put("Text", "// TODO: refactor this method");

        todos.add(todo);

        try (FileWriter fileWriter = new FileWriter("todos.json")) {
            fileWriter.write(todos.toJSONString());
        } catch (IOException e) {
            e.printStackTrace();
        }
    }
}