#!/usr/bin/python

from bottle import route, response, request, run, static_file, template

import sys
import os
import json

try:
    DEFAULT_SERVED_NCCO = sys.argv[1]
except IndexError:
    DEFAULT_SERVED_NCCO = 'inbound_call.json'

RESULT_NCCO=DEFAULT_SERVED_NCCO
RESULT_NCCO_2=DEFAULT_SERVED_NCCO

#endpoint to get the ncco
@route('/vapi/test' ,method='POST')
def get_ncco():
    print(json.load(request.body))
    if check_valid_json(RESULT_NCCO):
        jsonR = json.loads(RESULT_NCCO)
    else:
        jsonR = json.load(open(os.path.join('./', RESULT_NCCO)))
    response.content_type = 'application/json'
    return json.dumps(jsonR)

#endpoint to get the ncco
@route('/vapi/test/2')
def get_ncco():
    if check_valid_json(RESULT_NCCO_2):
        jsonR = json.loads(RESULT_NCCO_2)
    else:
        jsonR = json.load(open(os.path.join('./', RESULT_NCCO_2)))
    response.content_type = 'application/json'
    return json.dumps(jsonR)


#endpoint to change the file provided while requesting the ncco
@route('/vapi/test/<filename:path>')
def change_test(filename):
    global RESULT_NCCO
    path='./'+filename
    localFile = filename
    if os.path.isfile(path):
       RESULT_NCCO=localFile
    return json.dumps({"new_test_file": path})

#endpoint to provide enter and exit sounds for the named conversation
@route("/vapi/conversation/sound/<type:path>")
def get_audio_file(type):
    if type == 'enter.mp3':
        result='enter.mp3'
    elif type == 'exit.mp3':
        result='exit.mp3'
    else:
        result='acdc_black_in_black.mp3'
    return static_file(result, root='./', mimetype='audio/mpeg3')

#endpoint to provide a custom ncco that is not a file
# @route('/vapi/test',method='POST')
# def set_new_ncco():
#     global RESULT_NCCO
#     requestJson = json.dumps(request.json)
#     if check_valid_json(requestJson):
#         RESULT_NCCO = requestJson
#         return json.dumps(RESULT_NCCO)
#     else:
#         return json.dumps("{Invalid json}")


#endpoint to provide a custom ncco that is not a file
@route('/vapi/test/2',method='POST')
def set_new_ncco():
    global RESULT_NCCO_2
    requestJson = json.dumps(request.json)
    if check_valid_json(requestJson):
        RESULT_NCCO_2 = requestJson
        return json.dumps(RESULT_NCCO_2)
    else:
        return json.dumps("{Invalid json}")

#endpoint to provide a custom ncco that is not a file
@route('/vapi/test/event',method='POST')
def events_endpoint():
    requestJson = json.dumps(request.json)
    print(requestJson)
    response.headers["blah"] = "thing"

@route('/vapi/test/event',method='GET')
def events_endpoint():
    requestJson = json.dumps(request.json)
    print(requestJson)
    response.headers["blah"] = "thing"

#endpoint to provide a custom ncco that is not a file
@route('/vapi/nvm/event',method='POST')
def events_endpoint():
    requestJson = json.dumps(request.json)
    print(requestJson)


def check_valid_json(verify):
    try:
        json.loads(verify)
    except ValueError:
        return False
    return True


run(host='0.0.0.0',port=6969,debug=True)