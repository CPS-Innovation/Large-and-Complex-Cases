# API endpoints

Middleware

- auth validation 
    - add username and token to the context object
    - maybe have a helper/extension that reads the context we care about from the context object as a strongly typed
- global error handling


```
 https://comples-cases.com/api/
    
    GET /file-system/(egress|net-app)/{*path}

    GET /file-system/egress/
        Response: <PaginatedResponse>{workspaceId, name }[]

    GET /file-system/egress/{workspaceId}/folder/{?folderId}
        Response: <PaginatedResponse>{entityId, name, isFolder, path }[]
     
    GET /file-system/egress/{workspaceId}/files/{?fileId} NOT EXPOSED REALLY
        Response: binary of file
     
    HEAD /file-system/egress/{workspaceId}/files/{?fileId}  FOR LOOKUP IF FILE EXISTS - NEEDS LOOKING UP IN EGRESS SPEC 
        Response: 200 or 404

    GET /file-system/net-app/{*path} (if bucket)
        Response: <PaginatedResponse>{entityId, name, isFolder, path }[]

    GET /file-system/net-app/{*path} (if file) NOT EXPOSED REALLY
        Response: binary of file

    HEAD /file-system/net-app/{*path} FOR LOOKUP IF FILE EXISTS
        Response: 200 or 404

    either:
    POST /transfers/ (NOT PREFERRED)
        Request: {
            "filePaths": {source, destinationSubPath} [] 
                [
                    {source: "/file-system/egress/78127389123/files/123123123123.txt", "destinationSubPath": "123123123123.txt" },
                    {source: "/file-system/egress/asdasdasdads/files/asdasdadsasdas.txt", "destinationSubPath": "/files/asdasdadsasdas.txt" },
                ]
            "destination": "/file-system/net-app/uuhuhfuiui/"
        }

        Response: {
            "transferId": "diowjiqwojeiqwjeioqwjeioqw11qweqwe"
        }

    or:
    POST /transfers/ (PREFERRED)
        Request: {
            "filePaths": {source, destination} [] 
                [
                    {"source": "/file-system/egress/78127389123/files/123123123123.txt", "destination":"/file-system/net-app/some-folder/78127389123/files/123123123123"},
                    {"source": "/file-system/egress/asasasasas/123123123124.txt", "destination":"/file-system/net-app/some-folder/123123123123.txt"} 
                ],
        }

        Response: {
            "transferId": "diowjiqwojeiqwjeioqwjeioqw11qweqwe"
        }

    GET /transfers/{transferId}
        Response: {filePath, Status (in-progress, completed, failed)}[]
            
```

# Transfer

Client: 
 - hit POST /transfers/ function endpoint, get token back
 - hit GET /transfers/{transferId} endpoint, get status array back (know if we are still waiting or not)
 - when complete, fire off GET calls for every open folder in the UI to refresh egress and netApp views (not sure, GDS might not have this)

Internal:
 - handle POST, have an array of filepaths
 - create an OperationIdRoot (fresh guid)
 - for each filePath (with no top-level Orchestrator as an experiment), 
    - create an OperationId {OperationIdRoot}/{fresh guid}
    - execute an Orchestration with that OperationId, payload {filePath, destination}
    - each orchestration (currently) has one activity.
    
 - the GET endpoint will do a query on OperationId root (OperationIdRoot*), pretty sure we can get this out and return statuses

 - consider using stream out - stream in thing that Polaris did (https://github.com/CPS-Innovation/Polaris/blob/ee5fc2b8790bdd2019a59d49ef8bbdf8309f8669/polaris-pipeline/Common/Streaming/HttpResponseMessageStream.cs#L8)

Zips - 
 - for first pass do not unzip, but we can/have started thinking about this
 - Detect zip using Content-Type header

UI:
    - folder a/
        - folder aa/ 
            - file 5 [*]
            - file 6 [*]
        - folder ab/ 
        - file 3 []
        - file 4 []
    - folder b/
    - file 1 []
    - file 2 [*]



```