package com.coolape.tool;

import java.io.DataInputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;

import android.content.Context;
import android.os.Environment;

public class FileHelper {
	public Context context;
	public String SDPATH;
	public String FILESPATH;
	public boolean hasSDCD;

	public FileHelper(Context context) {
		this.context = context;
		SDPATH = Environment.getExternalStorageDirectory().getPath() + "/";
		FILESPATH = this.context.getFilesDir().getPath() + "/";
		hasSDCD = Environment.getExternalStorageState().equals(
				android.os.Environment.MEDIA_MOUNTED);
	}

	public FileHelper() {
		context = null;
		FILESPATH = null;
		hasSDCD = Environment.getExternalStorageState().equals(
				android.os.Environment.MEDIA_MOUNTED);
		SDPATH = Environment.getExternalStorageDirectory().getPath() + "/";
	}

	/**
	 * 在SD卡上创建文件
	 * 
	 * @throws IOException
	 */
	public File creatSDFile(String fileName) throws IOException {
		File file = new File(SDPATH + fileName);
		if(!file.getParentFile().exists()) {
			file.getParentFile().mkdirs();
		}
		file.createNewFile();
		return file;
	}

	public Boolean existsSDFile(String fileName) {
		File file = new File(SDPATH + fileName);
		return file.exists();
	}

	/**
	 * 删除SD卡上的文件
	 * 
	 * @param fileName
	 */
	public boolean delSDFile(String fileName) {
		File file = new File(SDPATH + fileName);
		if (file == null || !file.exists() || file.isDirectory())
			return false;
		file.delete();
		return true;
	}

	/**
	 * 在SD卡上创建目录
	 * 
	 * @param dirName
	 */
	public File creatSDDir(String dirName) {
		File dir = new File(SDPATH + dirName);
		dir.mkdirs();
		return dir;
	}

	/**
	 * 删除SD卡上的目录
	 * 
	 * @param dirName
	 */
	public boolean delSDDir(String dirName) {
		File dir = new File(SDPATH + dirName);
		return delDir(dir);
	}

	/**
	 * 修改SD卡上的文件或目录名
	 * 
	 * @param fileName
	 */
	public boolean renameSDFile(String oldfileName, String newFileName) {
		File oleFile = new File(SDPATH + oldfileName);
		File newFile = new File(SDPATH + newFileName);
		return oleFile.renameTo(newFile);
	}

	/**
	 * 拷贝SD卡上的单个文件
	 * 
	 * @param path
	 * @throws IOException
	 */
	public boolean copySDFileTo(String srcFileName, String destFileName)
			throws IOException {
		File srcFile = new File(SDPATH + srcFileName);
		File destFile = new File(SDPATH + destFileName);
		return copyFileTo(srcFile, destFile);
	}

	/**
	 * 拷贝SD卡上指定目录的所有文件
	 * 
	 * @param srcDirName
	 * @param destDirName
	 * @return
	 * @throws IOException
	 */
	public boolean copySDFilesTo(String srcDirName, String destDirName)
			throws IOException {
		File srcDir = new File(SDPATH + srcDirName);
		File destDir = new File(SDPATH + destDirName);
		return copyFilesTo(srcDir, destDir);
	}

	/**
	 * 移动SD卡上的单个文件
	 * 
	 * @param srcFileName
	 * @param destFileName
	 * @return
	 * @throws IOException
	 */
	public boolean moveSDFileTo(String srcFileName, String destFileName)
			throws IOException {
		File srcFile = new File(SDPATH + srcFileName);
		File destFile = new File(SDPATH + destFileName);
		return moveFileTo(srcFile, destFile);
	}

	/**
	 * 移动SD卡上的指定目录的所有文件
	 * 
	 * @param srcDirName
	 * @param destDirName
	 * @return
	 * @throws IOException
	 */
	public boolean moveSDFilesTo(String srcDirName, String destDirName)
			throws IOException {
		File srcDir = new File(SDPATH + srcDirName);
		File destDir = new File(SDPATH + destDirName);
		return moveFilesTo(srcDir, destDir);
	}

	/*
	 * 将文件写入sd卡。如:writeSDFile("test.txt");
	 */
	public FileOutputStream writeSDFile(String fileName) throws IOException {
		File file = new File(SDPATH + fileName);
		if(!file.exists()) {
			creatSDFile(fileName);
		}
		FileOutputStream fos = new FileOutputStream(file);
		return (fos);
	}

	/*
	 * 在原有文件上继续写文件。如:appendSDFile("test.txt");
	 */
	public FileOutputStream appendSDFile(String fileName) throws IOException {
		File file = new File(SDPATH + fileName);
		if(!file.exists()) {
			creatSDFile(fileName);
		}
		FileOutputStream fos = new FileOutputStream(file, true); // 第二个参数为true时，为向文件尾追加内容
		return (fos);
	}

	/*
	 * 从SD卡读取文件。如:readSDFile("test.txt");
	 */
	public FileInputStream readSDFile(String fileName) throws IOException {
		File file = new File(SDPATH + fileName);
		FileInputStream fis = new FileInputStream(file);
		return (fis);
	}

	/**
	 * 打开sd卡上的文件
	 * @param fileName
	 * @return
	 * @throws IOException
	 */
	public File openSDFile(String fileName) throws IOException {
		return new File(SDPATH + fileName);
	}

	/**********************************************************************************************/
	/**********************************************************************************************/
	/*
	 * 建立私有文件
	 * 
	 * @param fileName
	 * 
	 * @return
	 * 
	 * @throws IOException
	 */
	public File creatDataFile(String fileName) throws IOException {
		if (context == null)
			return null;
		File file = new File(FILESPATH + fileName);
		if (file.exists()) {
			return file;
		}
		file.getParentFile().mkdirs();
		file.createNewFile();
		return file;
	}

	/**
	 * 建立私有目录
	 * 
	 * @param dirName
	 * @return
	 */
	public File creatDataDir(String dirName) {
		if (context == null)
			return null;
		File dir = new File(FILESPATH + dirName);
		if (dir.exists()) {
			return dir;
		}
		dir.mkdirs();
		return dir;
	}

	/*
	 * 判断文件或目录是否存在
	 */
	public boolean existsDataFile(String fileName) {
		File file = new File(FILESPATH + fileName);
		return file.exists();
	}

	public File dataFile(String fileName) {
		if (context == null)
			return null;
		File file = new File(FILESPATH + fileName);
		return file;
	}

	/**
	 * 删除私有文件
	 * 
	 * @param fileName
	 * @return
	 */
	public boolean delDataFile(String fileName) {
		if (context == null)
			return false;
		File file = new File(FILESPATH + fileName);
		return delFile(file);
	}

	/**
	 * 删除私有目录
	 * 
	 * @param dirName
	 * @return
	 */
	public boolean delDataDir(String dirName) {
		if (context == null)
			return false;
		File file = new File(FILESPATH + dirName);
		return delDir(file);
	}

	/**
	 * 更改私有文件名
	 * 
	 * @param oldName
	 * @param newName
	 * @return
	 */
	public boolean renameDataFile(String oldName, String newName) {
		if (context == null)
			return false;
		File oldFile = new File(FILESPATH + oldName);
		File newFile = new File(FILESPATH + newName);
		return oldFile.renameTo(newFile);
	}

	/**
	 * 在私有目录下进行文件复制
	 * 
	 * @param srcFileName
	 *            ： 包含路径及文件名
	 * @param destFileName
	 * @return
	 * @throws IOException
	 */
	public boolean copyDataFileTo(String srcFileName, String destFileName)
			throws IOException {
		if (context == null)
			return false;
		File srcFile = new File(FILESPATH + srcFileName);
		File destFile = new File(FILESPATH + destFileName);
		if (destFile.exists()) {
			delDataFile(destFileName);
		}
		return copyFileTo(srcFile, destFile);
	}

	public boolean copyDataFileTo(InputStream srcIS, String destFileName)
			throws IOException {
		if (context == null)
			return false;
		File destfile = new File(FILESPATH + destFileName);
		if (destfile.exists()) {
			delDataFile(destFileName);
		}
		FileOutputStream fos = new FileOutputStream(FILESPATH + destFileName);
		int readLen = 0;
		byte[] buf = new byte[1024];
		while ((readLen = srcIS.read(buf)) != -1) {
			fos.write(buf, 0, readLen);
		}
		fos.flush();
		fos.close();
		return true;
	}

	/**
	 * 复制私有目录里指定目录的所有文件
	 * 
	 * @param srcDirName
	 * @param destDirName
	 * @return
	 * @throws IOException
	 */
	public boolean copyDataFilesTo(String srcDirName, String destDirName)
			throws IOException {
		if (context == null)
			return false;
		File srcDir = new File(FILESPATH + srcDirName);
		File destDir = new File(FILESPATH + destDirName);
		return copyFilesTo(srcDir, destDir);
	}

	/**
	 * 移动私有目录下的单个文件
	 * 
	 * @param srcFileName
	 * @param destFileName
	 * @return
	 * @throws IOException
	 */
	public boolean moveDataFileTo(String srcFileName, String destFileName)
			throws IOException {
		if (context == null)
			return false;
		File srcFile = new File(FILESPATH + srcFileName);
		File destFile = new File(FILESPATH + destFileName);
		return moveFileTo(srcFile, destFile);
	}

	/**
	 * 移动私有目录下的指定目录下的所有文件
	 * 
	 * @param srcDirName
	 * @param destDirName
	 * @return
	 * @throws IOException
	 */
	public boolean moveDataFilesTo(String srcDirName, String destDirName)
			throws IOException {
		if (context == null)
			return false;
		File srcDir = new File(FILESPATH + srcDirName);
		File destDir = new File(FILESPATH + destDirName);
		return moveFilesTo(srcDir, destDir);
	}

	/*
	 * 将文件写入应用私有的files目录。如:writeFile("test.txt");
	 */
	public OutputStream writeFile(String fileName) throws IOException {
		// Log.d("===================", fileName);
		OutputStream os = null;
		try {
			os = context.openFileOutput(fileName, Context.MODE_PRIVATE);
		} catch (Exception e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		return (os);
	}

	public FileOutputStream writeDataFile(String fileName) throws IOException {
		File file = new File(FILESPATH + fileName);
		file.getParentFile().mkdirs();
		return new FileOutputStream(file);
	}

	/*
	 * 在原有文件上继续写文件。如:appendFile("test.txt");
	 */
	public FileOutputStream appendFile(String fileName) throws IOException {
		File file = new File(FILESPATH + fileName);
		file.getParentFile().mkdirs();
		FileOutputStream fos = new FileOutputStream(file, true); // 第二个参数为true时，为向文件尾追加内容
		return (fos);

	}

	/*
	 * 从应用的私有目录files读取文件。如:readFile("test.txt");
	 */
	public InputStream readFile(String fileName) throws IOException {
		InputStream is = context.openFileInput(fileName);
		return is;
	}

	/**********************************************************************************************************/
	/*********************************************************************************************************/
	/**
	 * 删除一个文件
	 * 
	 * @param file
	 * @return
	 */
	public boolean delFile(File file) {
		if (file.isDirectory())
			return false;
		return file.delete();
	}

	/**
	 * 删除一个目录（可以是非空目录）
	 * 
	 * @param dir
	 */
	public boolean delDir(File dir) {
		if (dir == null || !dir.exists() || dir.isFile()) {
			return false;
		}
		for (File file : dir.listFiles()) {
			if (file.isFile()) {
				file.delete();
			} else if (file.isDirectory()) {
				delDir(file);// 递归
			}
		}
		dir.delete();
		return true;
	}

	/**
	 * 拷贝一个文件,srcFile源文件，destFile目标文件
	 * 
	 * @param path
	 * @throws IOException
	 */
	public boolean copyFileTo(File srcFile, File destFile) throws IOException {
		if (srcFile.isDirectory() || destFile.isDirectory())
			return false;// 判断是否是文件
		FileInputStream fis = new FileInputStream(srcFile);
		FileOutputStream fos = new FileOutputStream(destFile);
		int readLen = 0;
		byte[] buf = new byte[1024];
		while ((readLen = fis.read(buf)) != -1) {
			fos.write(buf, 0, readLen);
		}
		fos.flush();
		fos.close();
		fis.close();
		return true;
	}

	/**
	 * 拷贝目录下的所有文件到指定目录
	 * 
	 * @param srcDir
	 * @param destDir
	 * @return
	 * @throws IOException
	 */
	public boolean copyFilesTo(File srcDir, File destDir) throws IOException {
		if (!srcDir.isDirectory() || !destDir.isDirectory())
			return false;// 判断是否是目录
		if (!destDir.exists()) {
			destDir.mkdirs();
		}
		File[] srcFiles = srcDir.listFiles();
		for (int i = 0; i < srcFiles.length; i++) {
			if (srcFiles[i].isFile()) {
				// 获得目标文件
				File destFile = new File(destDir.getPath() + "\\"
						+ srcFiles[i].getName());
				copyFileTo(srcFiles[i], destFile);
			} else if (srcFiles[i].isDirectory()) {
				File theDestDir = new File(destDir.getPath() + "\\"
						+ srcFiles[i].getName());
				copyFilesTo(srcFiles[i], theDestDir);
			}
		}
		return true;
	}

	/**
	 * 移动一个文件
	 * 
	 * @param srcFile
	 * @param destFile
	 * @return
	 * @throws IOException
	 */
	public boolean moveFileTo(File srcFile, File destFile) throws IOException {
		boolean iscopy = copyFileTo(srcFile, destFile);
		if (!iscopy)
			return false;
		delFile(srcFile);
		return true;
	}

	/**
	 * 移动目录下的所有文件到指定目录
	 * 
	 * @param srcDir
	 * @param destDir
	 * @return
	 * @throws IOException
	 */
	public boolean moveFilesTo(File srcDir, File destDir) throws IOException {
		if (!srcDir.isDirectory() || !destDir.isDirectory()) {
			return false;
		}
		File[] srcDirFiles = srcDir.listFiles();
		for (int i = 0; i < srcDirFiles.length; i++) {
			if (srcDirFiles[i].isFile()) {
				File oneDestFile = new File(destDir.getPath() + "\\"
						+ srcDirFiles[i].getName());
				moveFileTo(srcDirFiles[i], oneDestFile);
				delFile(srcDirFiles[i]);
			} else if (srcDirFiles[i].isDirectory()) {
				File oneDestFile = new File(destDir.getPath() + "\\"
						+ srcDirFiles[i].getName());
				moveFilesTo(srcDirFiles[i], oneDestFile);
				delDir(srcDirFiles[i]);
			}

		}
		return true;
	}

	public static final byte[] readFileFully(File f) {
		byte[] b = null;

		if (f == null)
			return b;

		try {
			int len = (int) f.length();
			b = new byte[len];
			@SuppressWarnings("resource")
			DataInputStream dis = new DataInputStream(new FileInputStream(f));
			dis.readFully(b);
			return b;
		} catch (IOException e) {
			e.printStackTrace();
		}
		return null;
	}
}
