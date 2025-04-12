package com.codehealth.scanners.java;

import java.util.*;
import java.io.*;
import org.json.simple.JSONArray;
import org.json.simple.JSONObject;

@SuppressWarnings("unchecked")
public class CyclomaticComplexityScanner {
    public static void scan(Map<String, String> filesContent, String outputPath) {
        JSONObject report = new JSONObject();
        JSONArray fileReports = new JSONArray();

        final int[] totalComplexity = {0};
        final int[] totalMethods = {0};

        for (Map.Entry<String, String> entry : filesContent.entrySet()) {
            String filePath = entry.getKey();
            String content = entry.getValue();

            JSONObject fileJson = new JSONObject();
            JSONArray methodsArray = new JSONArray();

            fileJson.put("file", filePath.replace("\\", "/"));  // Normalize path

            boolean inMethod = false;
            int complexity = 1;
            String methodName = null;

            String[] lines = content.split("\n");
            for (String rawLine : lines) {
                String line = rawLine.trim();

                if (line.matches(".*\\b(public|private|protected)?\\s+\\w+\\s+\\w+\\(.*\\)\\s*\\{?.*")) {
                    inMethod = true;
                    methodName = line.split("\\(")[0].replaceAll(".*\\s", "");
                    complexity = 1;
                    continue;
                }

                if (inMethod) {
                    if (line.contains("{")) complexity++;
                    if (line.contains("if") || line.contains("for") || line.contains("while") ||
                        line.contains("case") || line.contains("catch") ||
                        line.contains("&&") || line.contains("||") || line.contains("?")) {
                        complexity++;
                    }

                    if (line.contains("}")) {
                        JSONObject methodJson = new JSONObject();
                        methodJson.put("method", methodName);
                        methodJson.put("complexity", complexity);
                        methodsArray.add(methodJson);
                        totalComplexity[0] += complexity;
                        totalMethods[0]++;
                        inMethod = false;
                    }
                }
            }

            if (!methodsArray.isEmpty()) {
                fileJson.put("methods", methodsArray);
                fileReports.add(fileJson);
            }
        }

        report.put("files", fileReports);
        report.put("totalComplexity", totalComplexity[0]);
        report.put("averageComplexity",
                totalMethods[0] > 0 ?
                        (double) totalComplexity[0] / totalMethods[0] :
                        0.0);

        try (FileWriter writer = new FileWriter(outputPath)) {
            writer.write(report.toJSONString());
        } catch (IOException e) {
            e.printStackTrace();
        }
    }
}
