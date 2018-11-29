"""
requirements
pip install python-jose
"""

from base64 import urlsafe_b64encode
import os
import time
from jose import jwt


APPLICATION_ID = "974f70b3-c15a-42a4-bca9-d3e7ab17254c"
APPLICATION_PRIVATE_KEY = """
"""

# prepare the claims / payload
payload = {
    "iat": int(time.time()) - 3600,  # issued at
    "application_id": APPLICATION_ID,  # application id
    "jti": urlsafe_b64encode(os.urandom(64)).decode('utf-8'),
    "exp": int(time.time()) + 3600000,
    "acl": {'paths': {'/**': {}}}
}

# generate the token
TOKEN = jwt.encode(
   claims=payload,     
   key=APPLICATION_PRIVATE_KEY, algorithm='RS256')

# print the token
print "our token is: {0}".format(TOKEN)