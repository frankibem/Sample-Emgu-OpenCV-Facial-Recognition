# Sample-Emgu-OpenCV-Facial-Recognition
This sample shows how to use the Emgu OpenCV C# wrapper to perform simple facial recognition

###Perequisites
Emgu has to be installed for this to work. https://sourceforge.net/projects/emgucv/files/

## Training
1. Clone the project and open it in visual studio
2. You need a directory, RAW which contains unlabeled images. Another directory, DETECTED will contain 100 x 100 faces detected from the images in RAW.
3. Set Trainer as the startup project, the configuration to x64 and build.
4. From the bin directory, enter "prepare RAW DETECTED" to detect faces from the images in RAW and save them to DETECTED.
5. Manually inspect the images in DETECTED, deleting those that are not faces.
6. Rename each image using the format ID_NUM. All faces that belong to the same person should have the same ID. NUM can be a sequence of integers. We will refer to the folder with the labeled images as LABELED
7. From the bin directory, enter "train LABELED train.yaml" to train a recognizer using the ids and generate train.yaml
8. Create a .csv file, LABELS, with each line in the format ID,NAME. This is important for prediction.

## Predicting
1. Set RecognitionDemo as the startup project.
2. Replace the placeholders in MainWindow.xaml.cs with the paths to train.yaml and LABELS
3. Build and run the project
4. Select an image from your computer and click Recognize to recognize faces in the image.

Feel free to modify the code to get even better results.
HAPPY CODING!
